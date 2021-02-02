using Appology.Helpers;
using Appology.MiFinance.DTOs;
using Appology.MiFinance.Helpers;
using Appology.MiFinance.Model;
using Appology.MiFinance.Repository;
using Appology.MiFinance.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Appology.MiFinance.Service
{
    public interface ISpendingService
    {
        Task<IEnumerable<Spending>> GetAllAsync(SpendingRequestDTO request);
        Task MissedCreditCardInterestEntriesAsync();
        Task<int?> GetIdFromFinanceAsync(int Id);
        Task MakeSpendingFinanceless(int id, int catId, int? secondCatId);
        Task InsertAsync(SpendingDTO dto);
        (DateTime? Date, decimal Amount) ExpenseLastPaid(int financeId);
        Task<IEnumerable<SpendingSummaryDTO>> GetSpendingSummary(DateFilter dateFilter, bool summaryOverview = false);
        Task<IEnumerable<SpendingSummaryDTO>> GetSpendingSummaryOverview(DateFilter dateFilter);
        Task<IEnumerable<MonthComparisonChartVM>> GetSpendingsByCategoryAndMonthAsync(DateFilter dateFilter, int catId, bool isSecondCat, bool isFinance);
        Task<IEnumerable<string>> RecentMonzoSyncedTranIds(int max);
        Task<bool> MonzoTransactionExists(string transId);
    }

    public class SpendingService : ISpendingService
    {
        private readonly ISpendingRepository spendingRepository;
        private readonly IRemindersService reminderService;

        public SpendingService(
            ISpendingRepository spendingRepository,
            IRemindersService reminderService
            )
        {
            this.spendingRepository = spendingRepository ?? throw new ArgumentNullException(nameof(spendingRepository));
            this.reminderService = reminderService ?? throw new ArgumentNullException(nameof(reminderService));
        }

        public async Task<IEnumerable<Spending>> GetAllAsync(SpendingRequestDTO request)
        {
            var spendings = (await spendingRepository.GetAllAsync(request.DateFilter));
                
            if (request.CatId.HasValue)
            {
                spendings = spendings.Where(x => (request.IsFinance && x.FinanceId == request.CatId.Value) || (!request.IsFinance && (request.IsSecondCat ? x.SecondCatId == request.CatId.Value : x.CatId == request.CatId.Value)));
            }
            
            return spendings
                .OrderByDescending(x => x.Date)
                .ThenBy(x => x.Name)
                .ThenBy(x => x.Category);
        }

        public async Task<int?> GetIdFromFinanceAsync(int Id)
        {
            return await spendingRepository.GetIdFromFinanceAsync(Id);
        }

        // if finance is a one-off payment, we need to delete the finance once its paid
        // but not delete the item from spendings table. so we need to set catId accordingly
        // getting the value from the finance table and then set financeId to null
        public async Task MakeSpendingFinanceless(int id, int catId, int? secondCatId)
        {
            await spendingRepository.MakeSpendingFinanceless(id, catId, secondCatId);
        }

        public async Task InsertAsync(SpendingDTO dto) 
        {
            await spendingRepository.InsertAsync(dto);
        }

        public (DateTime? Date, decimal Amount) ExpenseLastPaid(int financeId)
        {
            return spendingRepository.ExpenseLastPaid(financeId);
        }

        public async Task MissedCreditCardInterestEntriesAsync()
        {
            var creditCards = new List<string>()
            {
                "aqua",
                "vanquis",
                "barclaycard",
                "luma",
                "capital one"
            };

            var results = new List<MissedEntries>();

            foreach (var card in creditCards)
            {
                var entries = (await spendingRepository.MissedCreditCardInterestEntriesAsync(card));

                if (entries.Any())
                {
                    var data = new MissedEntries()
                    {
                        Name = card,
                        Dates = entries.Select(x => (x.Month + "/" + x.Year)).ToArray()
                    };

                    results.Add(data);
                }
            }

            await reminderService.MissedEntriesAsync(results, "You have a missed credit card interest entry for");
        }

        public async Task<IEnumerable<SpendingSummaryDTO>> GetSpendingSummaryOverview(DateFilter dateFilter)
        {
            var specialCatsSpendingsSummary = (await spendingRepository.GetSpecialCatsSpendingsSummaryAsync(dateFilter));
            var dataSpecialCats = new List<SpendingSummaryDTO>();
            var monthsBetween = DateUtils.MonthsBetweenRanges(dateFilter);

            foreach (var item in specialCatsSpendingsSummary)
            {
                dataSpecialCats.Add(new SpendingSummaryDTO
                {
                    Cat1 = item.SuperCategory,
                    Total = item.Total,
                    IsSpecialCat = true,
                    Average = DateUtils.ShowAverage(dateFilter) ?
                        $"Averaged monthly: {Utils.ToCurrency(item.Total / monthsBetween ?? item.Total)}" :
                        Utils.ToCurrency(item.Total)
                });
            }

            return dataSpecialCats;
        }

        private string ComputeAverage(IEnumerable<MonthComparisonChartVM> monthlyFigures, DateFilter dateFilter)
        {
            var fillEmptyMonths = FinanceUtils.AddEmptyMonths(monthlyFigures.ToList(), dateFilter);
            var data = FinanceUtils.AveragedChartResults(fillEmptyMonths);
            int countMonths = FinanceUtils.CountChartMonths(data);
            decimal total = data.Sum(x => x.Total);
            string average = countMonths > 0 ? Utils.ToCurrency(total / countMonths) : null;
            return DateUtils.ShowAverage(dateFilter) && average != null ? $"Averaged monthly: {average}" : Utils.ToCurrency(total);
        }

        public async Task<IEnumerable<SpendingSummaryDTO>> GetSpendingSummary(DateFilter dateFilter, bool summaryOverview)
        {
            var spendingsSummary = await spendingRepository.GetSpendingsSummaryAsync(dateFilter);

            if (summaryOverview)
            {
                spendingsSummary = spendingsSummary.Where(x => x.SuperCatId1 == null && x.SuperCatId2 == null);
            }

            var secondCats = spendingsSummary
                .Where(x => x.Cat2 != null)
                .GroupBy(
                    p => new { p.CatId, p.Cat1, p.IsFinance, p.SecondTypeId },
                    p => new { p.SecondCatId, p.Cat2, p.Total },
                    (key, g) =>
                        new SpendingSummaryDTO
                        {
                            Cat1 = key.Cat1,
                            CatId = key.CatId,
                            SecondTypeId = key.SecondTypeId,
                            IsFinance = key.IsFinance,
                            Total = spendingsSummary
                                .Where(x => x.CatId == key.CatId && x.Cat1 == key.Cat1)
                                .Sum(x => x.Total),
                            SecondCats = g.Select(s => new SpendingSummaryDTO
                            {
                                SecondCatId = s.SecondCatId,
                                Cat2 = s.Cat2,
                                Total = s.Total
                            }),
                        }
                 );

            var firstCats = spendingsSummary.Where(x => x.Cat2 == null);
            var data = firstCats.Concat(secondCats).OrderByDescending(x => x.Total).ToArray();
            var spendingsByMonth = await spendingRepository.GetSpendingsByMonthAsync(dateFilter);

            if (summaryOverview)
            {
                spendingsByMonth = spendingsByMonth.Where(x => x.SuperCatId1 == null && x.SuperCatId2 == null && x.FinanceSuperCatId == null);
            }

            foreach (var item in data)
            {
                if (item.Cat2 == null)
                {
                    var monthlyFigures = spendingsByMonth.Where(x => x.CatId == item.CatId && x.IsFinance == item.IsFinance);
                    item.Average = ComputeAverage(monthlyFigures, dateFilter);
                }
                else
                {

                    foreach (var x in item.SecondCats)
                    {
                        var monthlyFigures = spendingsByMonth.Where(d => d.SecondCatId == x.SecondCatId && x.IsFinance == false);
                        item.Average = ComputeAverage(monthlyFigures, dateFilter);
                    }
                }
            }

            return data;
        }

        public async Task<IEnumerable<MonthComparisonChartVM>> GetSpendingsByCategoryAndMonthAsync(DateFilter dateFilter, int catId, bool isSecondCat, bool isFinance)
        {
            var data = await spendingRepository.GetSpendingsByCategoryAndMonthAsync(dateFilter, catId, isSecondCat, isFinance);
            var fillEmptyMonths = FinanceUtils.AddEmptyMonths(data.ToList(), dateFilter);
            return fillEmptyMonths;

        }

        public async Task<IEnumerable<string>> RecentMonzoSyncedTranIds(int max)
        {
            return (await spendingRepository.RecentMonzoSyncedTranIds(max)).Select(x => x.MonzoTransId);
        }

        public async Task<bool> MonzoTransactionExists(string transId)
        {
            return await spendingRepository.MonzoTransactionExists(transId);
        }
    }
}
