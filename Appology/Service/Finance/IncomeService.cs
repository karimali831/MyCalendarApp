using Appology.MiFinance.DTOs;
using Appology.MiFinance.Model;
using Appology.MiFinance.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using Nager.Date;
using Nager.Date.Extensions;
using System.Threading.Tasks;
using Appology.MiFinance.Enums;
using Appology.MiFinance.Helpers;
using Appology.MiFinance.ViewModels;
using Appology.Helpers;

namespace Appology.MiFinance.Service
{
    public interface IIncomeService
    {
        Task<IEnumerable<Income>> GetAllIncomesAsync(IncomeRequestDTO request);
        Task<IEnumerable<IncomeSummaryDTO>> GetIncomeSummaryAsync(DateFilter dateFilter);
        Task InsertIncomeAsync(IncomeDTO dto);
        Task MissedIncomeEntriesAsync();
        Task<IEnumerable<MonthComparisonChartVM>> GetIncomesByCategoryAndMonthAsync(DateFilter dateFilter, int catId, bool isSecondCat);
        Task<IEnumerable<string>> RecentMonzoSyncedTranIds(int max);
    }

    public class IncomeService : IIncomeService
    {
        private readonly IFinanceRepository financeRepository;
        private readonly IIncomeRepository incomeRepository;
        private readonly ISpendingService spendingService;
        private readonly IRemindersService remindersService;
        private readonly IBaseService baseService;
        private readonly IRemindersService reminderService;

        public IncomeService(
            IFinanceRepository financeRepository,
            IIncomeRepository incomeRepository,
            ISpendingService spendingService,
            IRemindersService remindersService,
            IBaseService baseService,
            IRemindersService reminderService)
        {
            this.financeRepository = financeRepository ?? throw new ArgumentNullException(nameof(financeRepository));
            this.incomeRepository = incomeRepository ?? throw new ArgumentNullException(nameof(incomeRepository));
            this.spendingService = spendingService ?? throw new ArgumentNullException(nameof(spendingService));
            this.remindersService = remindersService ?? throw new ArgumentNullException(nameof(remindersService));
            this.baseService = baseService ?? throw new ArgumentNullException(nameof(baseService));
            this.reminderService = reminderService ?? throw new ArgumentNullException(nameof(reminderService));
        }

        public DateTime CalculateNextDueDate(DateTime date)
        {
            while (DateSystem.IsPublicHoliday(date, CountryCode.GB) || date.IsWeekend(CountryCode.GB))
            {
                date = date.AddDays(1);
            }

            return date;
        }


        public async Task<IEnumerable<Income>> GetAllIncomesAsync(IncomeRequestDTO request)
        {
            var incomes = (await incomeRepository.GetAllAsync(request.DateFilter));

            if (request.SourceId.HasValue)
            {
                incomes = incomes.Where(x => request.IsSecondCat ? x.SecondSourceId == request.SourceId.Value : x.SourceId == request.SourceId.Value);
            }

            return incomes.OrderByDescending(x => x.Date).ThenBy(x => x.Source);
        }

        public async Task<IEnumerable<IncomeSummaryDTO>> GetIncomeSummaryAsync(DateFilter dateFilter)
        {
            var incomeSummary = await incomeRepository.GetSummaryAsync(dateFilter);

            var secondCats = incomeSummary
                .Where(x => x.Cat2 != null)
                .GroupBy(
                    p => new { p.CatId, p.Cat1, p.SecondTypeId },
                    p => new { p.SecondCatId, p.Cat2, p.Total },
                    (key, g) =>
                        new IncomeSummaryDTO
                        {
                            Cat1 = key.Cat1,
                            CatId = key.CatId,
                            SecondTypeId = key.SecondTypeId,
                            Total = incomeSummary
                                .Where(x => x.CatId == key.CatId && x.Cat1 == key.Cat1)
                                .Sum(x => x.Total),
                                    SecondCats = g.Select(s => new IncomeSummaryDTO
                                    {
                                        SecondCatId = s.SecondCatId,
                                        Cat2 = s.Cat2,
                                        Total = s.Total
                                    }),
                        }
                 );

            var firstCats = incomeSummary.Where(x => x.Cat2 == null);
            var data = firstCats.Concat(secondCats).OrderByDescending(x => x.Total).ToArray();

            foreach (var item in data)
            {
                if (item.Cat2 == null)
                {
                    item.Average = DateUtils.ShowAverage(dateFilter) ?
                        FinanceUtils.ChartsHeaderTitle(await GetIncomesByCategoryAndMonthAsync(dateFilter, item.CatId, isSecondCat: false), ChartHeaderTitleType.Monthly) :
                        Utils.ToCurrency(item.Total);
                }
                else
                {
                    foreach (var x in item.SecondCats)
                    {
                        item.Average = DateUtils.ShowAverage(dateFilter) ?
                            FinanceUtils.ChartsHeaderTitle(await GetIncomesByCategoryAndMonthAsync(dateFilter, item.SecondCatId, isSecondCat: true), ChartHeaderTitleType.Monthly) :
                            Utils.ToCurrency(item.Total);
                    }
                }
            }

            return data;
        }


        public async Task MissedIncomeEntriesAsync()
        {
            var incomeStreams = new List<(string DateColumn, int WeekArrears, Categories CategoryType, string RecsBegan)>()
            {
                //(nameof(Income.AmazonWeekCommencing), 3, Categories.CWTL, "2019-08-07"),
                //(nameof(Income.Date), 1, Categories.UberEats, "2019-08-07"),
                (nameof(Income.Date), 1, Categories.Deliveroo, "2021-02-17"),
                (nameof(Income.Date), 1, Categories.Flex, "2020-03-23")
            };

            var results = new List<MissedEntries>();

            foreach (var incomeStream in incomeStreams)
            {
                var entries = (
                    await incomeRepository.MissedIncomeEntriesAsync(
                        incomeStream.DateColumn, 
                        incomeStream.WeekArrears, 
                        incomeStream.CategoryType,
                        incomeStream.RecsBegan
                    )
                );

                if (entries.Any())
                {
                    var data = new MissedEntries()
                    {
                        Name = incomeStream.CategoryType.ToString(),
                        Dates = entries.Select(x => (x.Year + " Week: " + x.Week)).ToArray()
                    };

                    results.Add(data);
                }
            }

            await reminderService.MissedEntriesAsync(results, "You have a missed income entry for");
        }

        public async Task<IEnumerable<MonthComparisonChartVM>> GetIncomesByCategoryAndMonthAsync(DateFilter dateFilter, int catId, bool isSecondCat)
        {
            var data = await incomeRepository.GetIncomesByCategoryAndMonthAsync(dateFilter, catId, isSecondCat);
            return FinanceUtils.AddEmptyMonths(data.ToList(), dateFilter);
        }

        public async Task InsertIncomeAsync(IncomeDTO dto)
        {
            await incomeRepository.InsertAsync(dto);
        }

        public async Task<IEnumerable<string>> RecentMonzoSyncedTranIds(int max)
        {
            return await incomeRepository.RecentMonzoSyncedTranIds(max);
        }
    }
}
