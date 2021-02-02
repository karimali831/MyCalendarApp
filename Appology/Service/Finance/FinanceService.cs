using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nager.Date;
using Nager.Date.Extensions;
using System.Globalization;
using Appology.MiFinance.Model;
using Appology.MiFinance.DTOs;
using Appology.MiFinance.Enums;
using Appology.MiFinance.Repository;
using Appology.Enums;
using Categories = Appology.MiFinance.Enums.Categories;
using Appology.Helpers;
using Appology.MiFinance.ViewModels;

namespace Appology.MiFinance.Service
{
    public interface IFinanceService
    {
        Task<Finance> GetAsync(int financeId);
        Task<IEnumerable<Finance>> GetAllAsync(bool resyncNextDueDates);
        Task<IEnumerable<FinanceVM>> GetFinances(bool resyncNextDueDates);
        Task InsertAsync(FinanceDTO dto);
        int? CalculateDays(DateTime? Date1, DateTime? Date2);
        int? DaysLastPaid(int Id);
        PaymentStatus PaymentStatusAsync(int Id, DateTime? nextDueDate, DateTime? endDate);
        Task<IEnumerable<MonthComparisonChartVM>> GetIncomeExpenseTotalsByMonth(DateFilter filter);
        Task<RemindersVM> GetNotifications();
        Task<IEnumerable<MonthComparisonChartVM>> GetFinanceTotalsByMonth(MonthComparisonChartRequestDTO request);
        Task<Summary> GetSummary();
    }

    public class FinanceService : IFinanceService
    {
        private readonly IFinanceRepository financeRepository;
        private readonly ISpendingService spendingService;
        private readonly IIncomeService incomeService;
        private readonly IRemindersService remindersService;
        private readonly IBaseService baseService;

        public FinanceService(
            IFinanceRepository financeRepository,
            ISpendingService spendingService,
            IIncomeService incomeService,
            IRemindersService remindersService,
            IBaseService baseService)
        {
            this.financeRepository = financeRepository ?? throw new ArgumentNullException(nameof(financeRepository));
            this.spendingService = spendingService ?? throw new ArgumentNullException(nameof(spendingService));
            this.incomeService = incomeService ?? throw new ArgumentNullException(nameof(incomeService));
            this.remindersService = remindersService ?? throw new ArgumentNullException(nameof(remindersService));
            this.baseService = baseService ?? throw new ArgumentNullException(nameof(baseService));
        }

        public DateTime CalculateNextDueDate(DateTime date)
        {
            while (DateSystem.IsPublicHoliday(date, CountryCode.GB) || date.IsWeekend(CountryCode.GB))
            {
                date = date.AddDays(1);
            }

            return date;
        }

        public async Task<IEnumerable<FinanceVM>> GetFinances(bool resyncNextDueDates)
        {
            var finances = await GetAllAsync(resyncNextDueDates);
            var viewModel = new List<FinanceVM>();

            if (finances.Any())
            {
                foreach (var x in finances)
                {
                    viewModel.Add(new FinanceVM
                    {
                        Id = x.Id,
                        Name = x.Name,
                        AvgMonthlyAmount = x.AvgMonthlyAmount,
                        Remaining = x.Remaining,
                        MonthlyDueDate = x.MonthlyDueDate,
                        EndDate = x.EndDate,
                        NextDueDate = x.NextDueDate,
                        OverrideNextDueDate = x.OverrideNextDueDate,
                        ManualPayment = x.ManualPayment,
                        DaysUntilDue = CalculateDays(x.NextDueDate, DateTime.UtcNow),
                        PaymentStatus = PaymentStatusAsync(x.Id, x.NextDueDate, x.EndDate),
                        DirectDebit = x.DirectDebit,
                        MonzoTag = x.MonzoTag,
                        SuperCatId = x.SuperCatId
                    });
                }
            }

            return viewModel
                .OrderByDescending(x => x.PaymentStatus)
                .ThenByDescending(x => (x.PaymentStatus == PaymentStatus.Late ? x.DaysUntilDue : null))
                .ThenBy(x => (x.PaymentStatus == PaymentStatus.Upcoming ? x.DaysUntilDue : null))
                .ThenBy(x => x.Name);
        }

        public async Task<RemindersVM> GetNotifications()
        {
            await incomeService.MissedIncomeEntriesAsync();
            //await spendingService.MissedCreditCardInterestEntriesAsync();
            //await cnwService.MissedCNWPaymentEntriesAsync();

            var finances = (await GetFinances(resyncNextDueDates: false)).Where(x => x.PaymentStatus != PaymentStatus.Ended);
            var upcomingPayments = upcomingPaymentRemindersAsync(finances);
            var getReminders = await remindersService.GetAllAsync();

            if (getReminders.Any())
            {
                foreach (var reminder in getReminders)
                {
                    reminder.DaysUntilDue = CalculateDays(reminder.DueDate, DateTime.UtcNow);
                }
            }

            var reminders = getReminders
                .Concat(upcomingPayments)
                .Where(x => x.Display == true)
                .OrderBy(x => x.Sort)
                .ThenBy(x => x.DueDate)
                .ThenByDescending(x => x._priority);

            return new RemindersVM
            {
                OverDueReminders = reminders.Where(x => x.PaymentStatus == PaymentStatus.Late),
                DueTodayReminders = reminders.Where(x => x.PaymentStatus == PaymentStatus.DueToday),
                UpcomingReminders = reminders.Where(x => x.PaymentStatus == PaymentStatus.Upcoming),
                Alerts = reminders.Where(x => x.PaymentStatus == PaymentStatus.Unknown)
            };
        }


        public async Task<Summary> GetSummary()
        {

            var settings = await baseService.GetSettingsAsync();

            var accrualIncome = (await incomeService.GetAllIncomesAsync(
                new IncomeRequestDTO
                {
                    DateFilter = new DateFilter
                    {
                        Frequency = DateFrequency.DateRange,
                        FromDateRange = settings.StartingDate,
                        ToDateRange = DateTime.UtcNow
                    }
                }));

            var accrualSpendings = (await spendingService.GetAllAsync(
                new SpendingRequestDTO
                {
                    DateFilter = new DateFilter
                    {
                        Frequency = DateFrequency.DateRange,
                        FromDateRange = settings.StartingDate,
                        ToDateRange = DateTime.UtcNow
                    }
                }));

            decimal incomeExcludingSavings = accrualIncome
                .Where(x => x.SourceId != (int)Categories.SavingsPot)
                .Sum(x => x.Amount);

            decimal incomeSavings = accrualIncome
                .Where(x => x.SourceId == (int)Categories.SavingsPot)
                .Sum(x => x.Amount);

            decimal cashBalance = accrualSpendings.Where(x => x.CashExpense).Sum(x => x.Amount);
            decimal cardBalance = accrualSpendings.Where(x => !x.CashExpense).Sum(x => x.Amount);

            decimal remainingCardBalance = (settings.AvailableCredit + incomeExcludingSavings) - cardBalance;
            decimal remainingCashBalance = settings.AvailableCash - cashBalance;

            return
                new Summary
                {
                    EstimatedBalance = Utils.ToCurrency(remainingCardBalance),
                    RemainingCash = Utils.ToCurrency(remainingCashBalance),
                    AccruedSavings = Utils.ToCurrency(incomeSavings)
                };
        }

        public async Task<Finance> GetAsync(int financeId)
        {
            return await financeRepository.GetAsync(financeId);
        }

        public async Task<IEnumerable<Finance>> GetAllAsync(bool resyncNextDueDates)
        {
            var finances = await financeRepository.GetAllAsync();

            if (finances.Any())
            {
                foreach(var finance in finances)
                {
                    var expenseLastPaid = spendingService.ExpenseLastPaid(finance.Id);

                    // if average monthly amount varies get the amount from last paid amount
                    if (finance.AvgMonthlyAmount == 0 && expenseLastPaid.Date.HasValue)
                    {
                        finance.AvgMonthlyAmount = expenseLastPaid.Amount;
                    }

                    // delete finance if one-off payment and is paid (i.e. ManualPayment = 1)
                    if (finance.ManualPayment && (!expenseLastPaid.Date.HasValue || DateTime.UtcNow.Date >= expenseLastPaid.Date.Value))
                    {
                        int? financePaidId = await spendingService.GetIdFromFinanceAsync(finance.Id);

                        if (financePaidId.HasValue)
                        {
                            await spendingService.MakeSpendingFinanceless(financePaidId.Value, finance.CatId, finance.SecondCatId);
                            await baseService.DeleteAsync(finance.Id, Table.Finances);
                        }
                    }

                    // auto set next due date - refactor this!v 
                    if ((finance.OverrideNextDueDate != OverrideDueDate.No || finance.NextDueDate == null) && finance.MonthlyDueDate.HasValue && finance.MonthlyDueDate != 0)
                    {
                        if (finance.NextDueDate == null || DateTime.UtcNow.Date >= finance.NextDueDate || resyncNextDueDates)
                        {
                            // don't set next due date if previous month not paid !
                            if (finance.NextDueDate == null || !expenseLastPaid.Date.HasValue || DateTime.UtcNow.Date >= expenseLastPaid.Date.Value)
                            {
                                int monthElapsed = finance.MonthlyDueDate >= DateTime.UtcNow.Day ? 0 : 1;
                                var previousDueDate = expenseLastPaid.Date.HasValue ? expenseLastPaid.Date.Value.Date : DateTime.UtcNow;
                                var nextDueMonth = expenseLastPaid.Date.HasValue ? previousDueDate.AddMonths(1).Month : previousDueDate.AddMonths(monthElapsed).Month;
                
                                // i.e. 31/10/2019 cannot set next due date to 31/11/2019 so check last day of the month
                                var lastDayInPreviousMonth = DateTime.DaysInMonth(DateTime.UtcNow.Year, previousDueDate.Month);
                                var lastDayInNextMonth = DateTime.DaysInMonth(DateTime.UtcNow.Year, nextDueMonth);

                                if (previousDueDate.Day == lastDayInPreviousMonth && lastDayInPreviousMonth > lastDayInNextMonth)
                                {
                                    finance.MonthlyDueDate = lastDayInNextMonth;
                                }

                                var nextDueDate = $"{nextDueMonth}-{finance.MonthlyDueDate}-{DateTime.UtcNow.ToString("yyyy", CultureInfo.InvariantCulture)}";

                                if (DateTime.TryParseExact(nextDueDate, "M-d-yyyy", CultureInfo.CurrentUICulture, DateTimeStyles.None, out DateTime date))
                                { 
                                    var calcDate = finance.OverrideNextDueDate == OverrideDueDate.WorkingDays ? CalculateNextDueDate(date) : date;
                                    await financeRepository.UpdateNextDueDateAsync(calcDate, finance.Id);
                                }
                            }
                        }
                    }
                }
            }

            return finances;
        }

        public int? CalculateDays(DateTime? Date1, DateTime? Date2)
        {
            if (!Date1.HasValue || !Date2.HasValue)
            {
                return null;
            }

            return (int)(Date1.Value.Date - Date2.Value.Date).TotalDays;
        }

        public int? DaysLastPaid(int Id)
        {
            var expenseLastPaidDate = spendingService.ExpenseLastPaid(Id);
            return CalculateDays(DateTime.UtcNow, expenseLastPaidDate.Date);
        }

        public PaymentStatus PaymentStatusAsync(int Id, DateTime? nextDueDate, DateTime? endDate)
        {
            if (!nextDueDate.HasValue)
            {
                return PaymentStatus.Unknown;
            }

            var daysUntilDue = CalculateDays(nextDueDate, DateTime.UtcNow);
            var daysLastPaid = DaysLastPaid(Id);

            if (endDate.HasValue && DateTime.UtcNow.Date > endDate.Value)
            {
                return PaymentStatus.Ended;
            }

            if (daysUntilDue == null)
            {
                return PaymentStatus.Unknown;
            }

            if (daysLastPaid <= 7)
            {
                return PaymentStatus.Paid;
            }
            else if (daysUntilDue < 0)
            {
                return PaymentStatus.Late;
            }
            else if (daysUntilDue == 0)
            {
                return PaymentStatus.DueToday;
            }
            else if (daysUntilDue > 0)
            {
                return PaymentStatus.Upcoming;
            }
            else
            {
                return PaymentStatus.Unknown;
            }
        }

        public async Task<IEnumerable<MonthComparisonChartVM>> GetIncomeExpenseTotalsByMonth(DateFilter filter)
        {
            return await financeRepository.GetIncomeExpenseTotalsByMonth(filter);
        }

        public async Task<IEnumerable<MonthComparisonChartVM>> GetFinanceTotalsByMonth(MonthComparisonChartRequestDTO request)
        {
            return await financeRepository.GetFinanceTotalsByMonth(request);
        }

        public async Task InsertAsync(FinanceDTO dto)
        {
            await financeRepository.InsertAsync(dto);
        }

        private IEnumerable<Reminder> upcomingPaymentRemindersAsync(IEnumerable<FinanceVM> finances)
        {
            (int Count, decimal Total) latePayments = (finances
                    .Count(x => x.PaymentStatus == PaymentStatus.Late), finances
                    .Where(x => x.PaymentStatus == PaymentStatus.Late)
                    .Sum(x => x.AvgMonthlyAmount));

            (int Count, decimal Total) upcomingPayments = (finances
                    .Count(x => x.PaymentStatus == PaymentStatus.Upcoming && (x.NextDueDate <= DateTime.UtcNow.Date.AddDays(7) || x.ManualPayment)), finances
                    .Where(x => x.PaymentStatus == PaymentStatus.Upcoming && (x.NextDueDate <= DateTime.UtcNow.Date.AddDays(7) || x.ManualPayment))
                    .Sum(x => x.AvgMonthlyAmount));

            (int Count, decimal Total) dueTodayPayments = (finances
                    .Count(x => x.PaymentStatus == PaymentStatus.DueToday), finances
                    .Where(x => x.PaymentStatus == PaymentStatus.DueToday)
                    .Sum(x => x.AvgMonthlyAmount));

            var reminders = new List<(string Note, PaymentStatus PaymentStatus)>();

            if (latePayments.Count > 0)
            {
                reminders.Add(($"You have {latePayments.Count} late payments totalling {Utils.ToCurrency(latePayments.Total)}", PaymentStatus.Late));
            }

            if (upcomingPayments.Count > 0)
            {
                reminders.Add(($"You have {upcomingPayments.Count} upcoming payments totalling {Utils.ToCurrency(upcomingPayments.Total)}", PaymentStatus.Upcoming));
            }

            if (dueTodayPayments.Count > 0)
            {
                reminders.Add(($"You have {dueTodayPayments.Count} payments due today totalling {Utils.ToCurrency(dueTodayPayments.Total)}", PaymentStatus.DueToday));
            }

            var paymentReminders = new List<Reminder>();

            foreach (var reminder in reminders)
            {
                paymentReminders.Add(new Reminder
                {
                    Notes = reminder.Note,
                    PaymentStatus = reminder.PaymentStatus,
                    _priority = Priority.High,
                    Category = Categories.Bills.ToString(),
                    Display = true,
                    Sort = 0
                });
            }

            return paymentReminders;
        }
    }
}
