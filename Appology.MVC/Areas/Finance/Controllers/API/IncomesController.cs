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
    [RoutePrefix("api/incomes")]
    [CamelCaseControllerConfig]
    public class IncomesController : ApiController
    {
        private readonly IIncomeService incomeService;
        private readonly IBaseService baseService;

        public IncomesController(IIncomeService incomeService, IBaseService baseService)
        {
            this.incomeService = incomeService ?? throw new ArgumentNullException(nameof(incomeService));
            this.baseService = baseService ?? throw new ArgumentNullException(nameof(baseService));
        }


        [Route("")]
        [HttpPost]
        public async Task<HttpResponseMessage> GetIncomesAsync(IncomeRequestDTO request)
        {
            var incomes = (await incomeService.GetAllIncomesAsync(request))
                .Where(x => x.SourceId != (int)Categories.SavingsPot);

            return Request.CreateResponse(HttpStatusCode.OK, new {
                Incomes = 
                    incomes.Select(x => new
                    {
                        x.Id,
                        x.Name,
                        x.Source,
                        x.SecondSource, 
                        Date = x.Date.ToString("MM/dd/yyyy"),
                        x.Amount,
                        x.WeekNo
                    })
            });;;
        }

        [Route("summary")]
        [HttpPost]
        public async Task<HttpResponseMessage> GetIncomesSummaryAsync(DateFilter dateFilter)
        {
            if (DateTime.TryParseExact(dateFilter.FromDateRange.ToString(), "dd-MM-yy", new CultureInfo("en-GB"), DateTimeStyles.None, out DateTime fromDateRange))
            {
                dateFilter.FromDateRange = fromDateRange;
            }

            if (DateTime.TryParseExact(dateFilter.ToDateRange.ToString(), "dd-MM-yy", new CultureInfo("en-GB"), DateTimeStyles.None, out DateTime toDateRange))
            {
                dateFilter.ToDateRange = toDateRange;
            }

            var incomeSummary = await incomeService.GetIncomeSummaryAsync(dateFilter);

            return Request.CreateResponse(HttpStatusCode.OK, 
                new {
                    IncomeSummary = incomeSummary,
                    TotalIncome = incomeSummary.Sum(x => x.Total)
                }
            );
        }

        [Route("add")]
        [HttpPost]
        public async Task<HttpResponseMessage> InsertIncomeAsync(IncomeDTO model)
        {
            await incomeService.InsertIncomeAsync(model);
            return Request.CreateResponse(HttpStatusCode.OK, true);
        }

        [Route("chart")]
        [HttpPost]
        public async Task<HttpResponseMessage> IncomesByCategoryChart(MonthComparisonChartRequestDTO request)
        {
            var isSecondCat = request.SecondCatId.HasValue && request.SecondCatId != 0 && request.SecondCatId != 9999 ? true : false;
            var catId = isSecondCat ? request.SecondCatId.Value : request.CatId;
            var catName = await baseService.GetCategoryName(catId);

            var dictionary = new Dictionary<string, MonthComparisonChartVM[]>()
            {
                { catName, (await incomeService.GetIncomesByCategoryAndMonthAsync(request.DateFilter, catId, isSecondCat)).ToArray() }
            };

            var results = dictionary.Values.ToList();
        
            if (!results[0].Any())
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No results");
            }
      
            string secondCategory = string.IsNullOrEmpty(results[0].First().SecondCategory) ? "" : $"- ({results[0].First().SecondCategory})";

            var summary = new List<ChartSummaryVM>()
            {
                new ChartSummaryVM
                {
                    Title = string.Format("{0} Chart for {1} {2}", "Incomes", results[0].First().Category, secondCategory),
                    AveragedDaily = FinanceUtils.ChartsHeaderTitle(results[0], ChartHeaderTitleType.Daily),
                    AveragedMonthly = FinanceUtils.ChartsHeaderTitle(results[0], ChartHeaderTitleType.Monthly),
                    TotalSpent = FinanceUtils.ChartsHeaderTitle(results[0], ChartHeaderTitleType.Total)
                }
            };

            if (request.SecondCatId.HasValue && request.SecondCatId.Value == 9999)
            {
                int secondTypeId = await baseService.GetSecondTypeId(catId);
                var categories = await baseService.GetAllCategories((CategoryType)secondTypeId, false);

                if (categories.Any())
                {
                    foreach (var cat in categories)
                    {
                        dictionary.Add(cat.Name, (await incomeService.GetIncomesByCategoryAndMonthAsync(request.DateFilter, cat.Id, isSecondCat: true)).ToArray());
                    }
                }

                dictionary.Remove(catName);
            }

            return Request.CreateResponse(HttpStatusCode.OK,
                new ChartVM
                {
                    Summary = summary,
                    Labels = FinanceUtils.ChartLabels(results),
                    Data = dictionary
                });
        }
    }
}
