using Appology.Controllers.Api;
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
    [RoutePrefix("api/spendings")]
    [EnableCors(origins: "http://localhost:3000", headers: "*", methods: "*")]
    [CamelCaseControllerConfig]
    public class SpendingsController : ApiController
    {
        private readonly ISpendingService spendingService;
        private readonly IBaseService baseService;
        private readonly IFinanceService financeService;
        private readonly IIncomeService incomeService;

        public SpendingsController(
            ISpendingService spendingService, 
            IFinanceService financeService,
            IIncomeService incomeService,
            IBaseService baseService)
        {
            this.spendingService = spendingService ?? throw new ArgumentNullException(nameof(spendingService));
            this.financeService = financeService ?? throw new ArgumentNullException(nameof(financeService));
            this.incomeService = incomeService ?? throw new ArgumentNullException(nameof(incomeService));
            this.baseService = baseService ?? throw new ArgumentNullException(nameof(baseService));
        }

        [HttpPost]
        public async Task<HttpResponseMessage> GetSpendingsAsync(SpendingRequestDTO request)
        {
            var spendings = await spendingService.GetAllAsync(request);

            return Request.CreateResponse(HttpStatusCode.OK, new {
                Spendings = spendings.Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Amount,
                    Date = x.Date.ToString("MM/dd/yyyy"),
                    x.Info,
                    x.Category,
                    x.SecondCategory,
                    x.CashExpense
                })
            });
        }

        [Route("summary")]
        [HttpPost]
        public async Task<HttpResponseMessage> GetSpendingSummaryAsync(DateFilter dateFilter)
        {
            if (DateTime.TryParseExact(dateFilter.FromDateRange.ToString(), "dd-MM-yy", new CultureInfo("en-GB"), DateTimeStyles.None, out DateTime fromDateRange))
            {
                dateFilter.FromDateRange = fromDateRange;
            }

            if (DateTime.TryParseExact(dateFilter.ToDateRange.ToString(), "dd-MM-yy", new CultureInfo("en-GB"), DateTimeStyles.None, out DateTime toDateRange))
            {
                dateFilter.ToDateRange = toDateRange;
            }

            var spendingsSummary = await spendingService.GetSpendingSummary(dateFilter);
            var spendingsSummary2 = await spendingService.GetSpendingSummary(dateFilter, summaryOverview: true);
            var spendingsSummary3 = await spendingService.GetSpendingSummaryOverview(dateFilter);
            var spendingsOverview = spendingsSummary3.Concat(spendingsSummary2).OrderByDescending(x => x.Total);

            dateFilter.DateField = "PayDate";
   
            return Request.CreateResponse(HttpStatusCode.OK,
                new {
                    SpendingSummary = spendingsSummary,
                    SpendingSummaryOverview = spendingsOverview,
                    TotalSpent = spendingsSummary.Sum(x => x.Total)
                }
            );
        }

        [Route("add")]
        [HttpPost]
        public async Task<HttpResponseMessage> InsertAsync(SpendingDTO dto)
        {
            // hack for now 
            if (dto.FinanceId == 0)
                dto.FinanceId = null;

            if (dto.CatId == 0)
                dto.CatId = null;

            if (dto.FinanceId.HasValue && dto.CatId.HasValue)
                dto.CatId = null;

            if (!dto.FinanceId.HasValue && !dto.CatId.HasValue)
                return Request.CreateResponse(HttpStatusCode.BadRequest, false);
                
            if (dto.CatId == (int)Categories.Savings)
            {
                var potIncomeDto = new IncomeDTO
                {
                    Amount = dto.Amount,
                    SourceId = (int)Categories.SavingsPot,
                    Date = dto.Date,
                };

                await incomeService.InsertIncomeAsync(potIncomeDto);
            }

            await spendingService.InsertAsync(dto);
            return Request.CreateResponse(HttpStatusCode.OK, true);
        }

        [Route("chart")]
        [HttpPost]
        public async Task<HttpResponseMessage> SpendingsByCategoryChart(MonthComparisonChartRequestDTO request)
        {
            var isSecondCat = request.SecondCatId.HasValue && !request.IsFinance && request.SecondCatId != 0 && request.SecondCatId != 9999 ? true : false;
            var catId = isSecondCat ? request.SecondCatId.Value : request.CatId;

            var catName = request.IsFinance 
                ? (await financeService.GetAsync(catId)).Name
                : await baseService.GetCategoryName(catId);

            var dictionary = new Dictionary<string, MonthComparisonChartVM[]>
            {
                { catName, (await spendingService.GetSpendingsByCategoryAndMonthAsync(request.DateFilter, catId, isSecondCat, request.IsFinance)).ToArray() }
            };

            var results = dictionary.Values.ToList();

            if (!results[0].Any())
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No results");
            }


            string secondCategory = !isSecondCat ? "" : $"- ({results[0].First().SecondCategory})";

            var summaries = new List<ChartSummaryVM>
            {
                new ChartSummaryVM
                {
                    Title = string.Format("{0} Chart for {1} {2}", "Spendings", results[0].First().Category, secondCategory),
                    AveragedDaily = FinanceUtils.ChartsHeaderTitle(results[0], ChartHeaderTitleType.Daily),
                    AveragedMonthly = FinanceUtils.ChartsHeaderTitle(results[0], ChartHeaderTitleType.Monthly),
                    TotalSpent = FinanceUtils.ChartsHeaderTitle(results[0], ChartHeaderTitleType.Total)
                }
            };


            // make subcats datasets
            if (request.SecondCatId.HasValue && request.SecondCatId.Value == 9999)
            {
                int secondTypeId = await baseService.GetSecondTypeId(catId);
                var categories = await baseService.GetAllCategories((CategoryType)secondTypeId, false);

                if (categories.Any())
                {
                    foreach (var cat in categories)
                    {
                        dictionary.Add(cat.Name, (await spendingService.GetSpendingsByCategoryAndMonthAsync(request.DateFilter, cat.Id, isSecondCat: true, isFinance: false)).ToArray());
                    }
                }

                dictionary.Remove(catName);
            }


            return Request.CreateResponse(HttpStatusCode.OK, 
                new ChartVM
                {
                    Labels = FinanceUtils.ChartLabels(results),
                    Summary = summaries,
                    Data = dictionary
                }
            );
        }
    }
}
