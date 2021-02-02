using Appology.Controllers.Api;
using Appology.Enums;
using Appology.MiFinance.DTOs;
using Appology.MiFinance.Enums;
using Appology.MiFinance.Helpers;
using Appology.MiFinance.Service;
using Appology.MiFinance.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace Appology.Areas.MiFinance.Controllers.API
{
    [RoutePrefix("api/finances")]
    [EnableCors(origins: "http://localhost:3000", headers: "*", methods: "*")]
    [CamelCaseControllerConfig]
    public class FinancesController : ApiController
    {
        private readonly IFinanceService financeService;
        private readonly ISpendingService spendingService;

        public FinancesController(IFinanceService financeService, ISpendingService spendingService)
        {
            this.financeService = financeService ?? throw new ArgumentNullException(nameof(financeService));
            this.spendingService = spendingService ?? throw new ArgumentNullException(nameof(spendingService));
        }

        [Route("{resyncNextDueDates}/{upcomingPayments}")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetFinancesAsync(bool resyncNextDueDates, bool upcomingPayments)
        {
            var finances = await financeService.GetFinances(resyncNextDueDates);

            if (upcomingPayments)
            {
                finances = finances.Where(x => (x.PaymentStatus != PaymentStatus.Ended && x.NextDueDate <= DateTime.UtcNow.Date.AddDays(7)) || x.ManualPayment);
            }

            var currentMonth = Enum.TryParse(DateTime.UtcNow.Date.ToString("MMMM", CultureInfo.InvariantCulture), out DateFrequency thisMonth);
            var spentThisMonth = await spendingService.GetSpendingSummary(new DateFilter { Frequency = thisMonth, Interval = 1 });

            var lastMonth = Enum.TryParse(DateTime.UtcNow.Date.AddMonths(-1).ToString("MMMM", CultureInfo.InvariantCulture), out DateFrequency previousMonth);
            var spentLastMonth = await spendingService.GetSpendingSummary(new DateFilter { Frequency = previousMonth, Interval = 1 });

            return Request.CreateResponse(HttpStatusCode.OK, new {
                Finances =
                    finances.Select(x => new
                    {
                        x.Id,
                        x.Name,
                        x.AvgMonthlyAmount,
                        x.Remaining,
                        x.MonthlyDueDate,
                        x.OverrideNextDueDate,
                        x.ManualPayment,
                        x.DaysUntilDue,
                        x.PaymentStatus,
                        x.DirectDebit,
                        x.MonzoTag,
                        x.SuperCatId,
                        EndDate = x.EndDate.HasValue ? x.EndDate.Value.ToString("dd-MM-yy") : null,
                        NextDueDate = x.NextDueDate.HasValue ? x.NextDueDate.Value.ToLongDateString() : null
                    }),
                TotalAvgCost = finances
                    .Where(x => x.EndDate == null || DateTime.UtcNow.Date < x.EndDate)
                    .Sum(x => x.AvgMonthlyAmount),
                SpentThisMonth = spentThisMonth.Where(x => x.IsFinance == true).Sum(x => x.Total),
                SpentLastMonth = spentLastMonth.Where(x => x.IsFinance == true).Sum(x => x.Total)
            });
        }

        [Route("add")]
        [HttpPost]
        public async Task<HttpResponseMessage> InsertAsync(FinanceDTO model)
        {
            await financeService.InsertAsync(model);
            return Request.CreateResponse(HttpStatusCode.OK, true);
        }

        [Route("chart/incomeexpense")]
        [HttpPost]
        public async Task<HttpResponseMessage> IncomeExpenseComparisonChart(MonthComparisonChartRequestDTO request)
        {
            var summaries = new List<ChartSummaryVM>()
            {
                new ChartSummaryVM { Title = "Spending Summary" },
                new ChartSummaryVM { Title = "Income Summary" },
                new ChartSummaryVM { Title = "Savings Summary" }
            };

            var datasets = new Dictionary<string, MonthComparisonChartVM[]>
            {
                { summaries[0].Title, (await financeService.GetIncomeExpenseTotalsByMonth(request.DateFilter)).Where(x => x.Type == CategoryType.Spendings).ToArray() },
                { summaries[1].Title, (await financeService.GetIncomeExpenseTotalsByMonth(request.DateFilter)).Where(x => x.Type == CategoryType.Income).ToArray() },
                { summaries[2].Title, (await financeService.GetIncomeExpenseTotalsByMonth(request.DateFilter)).Where(x => x.Type == CategoryType.Savings).ToArray() }
            };

            var results = datasets.Values.ToList();
            var labels = FinanceUtils.ChartLabels(results);

            int idx = 0;
            foreach (var summary in summaries)
            {
                summaries[idx].AveragedDaily = FinanceUtils.ChartsHeaderTitle(results[idx], ChartHeaderTitleType.Daily);
                summaries[idx].AveragedMonthly = FinanceUtils.ChartsHeaderTitle(results[idx], ChartHeaderTitleType.Monthly);
                summaries[idx].TotalSpent = FinanceUtils.ChartsHeaderTitle(results[idx], ChartHeaderTitleType.Total);
                idx++;
            };


            return Request.CreateResponse(HttpStatusCode.OK,
                new ChartVM
                {
                    Labels = labels,
                    Summary = summaries,
                    Data = datasets
                });
        }

        [Route("chart")]
        [HttpPost]
        public async Task<HttpResponseMessage> FinancesChartAsync(MonthComparisonChartRequestDTO request)
        {
            var dictionary = new Dictionary<string, MonthComparisonChartVM[]>
            {
                {"Finances Breakdown", (await financeService.GetFinanceTotalsByMonth(request)).ToArray() }
            };

            var results = dictionary.Values.ToList();

            var summaries = new List<ChartSummaryVM>
            {
                new ChartSummaryVM
                {
                    Title = "Finances Breakdown",
                    AveragedDaily = FinanceUtils.ChartsHeaderTitle(results[0], ChartHeaderTitleType.Daily),
                    AveragedMonthly = FinanceUtils.ChartsHeaderTitle(results[0], ChartHeaderTitleType.Monthly),
                    TotalSpent = FinanceUtils.ChartsHeaderTitle(results[0], ChartHeaderTitleType.Total),
                }
            };

            return Request.CreateResponse(HttpStatusCode.OK, new ChartVM
            {
                Labels = FinanceUtils.ChartLabels(results),
                Summary = summaries,
                Data = dictionary
            });
        }
    }
}
