using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using Appology.MiFinance.Service;
using Appology.MiFinance.Enums;
using Appology.MiFinance.DTOs;
using Appology.MiFinance.Helpers;
using Appology.Enums;
using Appology.Controllers.Api;

namespace Appology.Areas.MiFinance.Controllers.API
{
    [RoutePrefix("api")]
    [CamelCaseControllerConfig]
    public class BaseCommonController : ApiController
    {
        private readonly IBaseService baseService;

        public BaseCommonController(IBaseService baseService)
        {
            this.baseService = baseService ?? throw new ArgumentNullException(nameof(baseService));
        }

        [Route("categories/{typeId?}/{catsWithSubs?}")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetCategoriesAsync(CategoryType? typeId = null, bool catsWithSubs = false)
        {
            var categories = await baseService.GetAllCategories(typeId, catsWithSubs);
            return Request.CreateResponse(HttpStatusCode.OK, new { Categories = categories });
        }

        [Route("categories/add")]
        [HttpPost]
        public async Task<HttpResponseMessage> AddCategoryAsync(CategoryDTO dto)
        {
            await baseService.AddCategory(dto);
            return Request.CreateResponse(HttpStatusCode.OK, true);
        }

        [Route("update")]
        [HttpPost]
        public async Task<HttpResponseMessage> UpdateAsync(UpdateRequest model)
        {
            if (Enum.TryParse(model.TableName, out Table table))
            {
                string fieldToPascal = FinanceUtils.FirstCharToUpper(model.Field);
                object dbValue = model.Value;

                if (DateTime.TryParseExact(model.Value, "yyyy-MM-dd", new CultureInfo("en-GB"), DateTimeStyles.None, out DateTime date))
                {
                    dbValue = date;
                }

                await baseService.UpdateAsync(fieldToPascal, dbValue, model.Id, table);
                return Request.CreateResponse(HttpStatusCode.OK, true);
            }

            return Request.CreateResponse(HttpStatusCode.BadRequest, false);

        }

        [Route("delete/{id}/{tableName}")]
        [HttpPost]
        public async Task<HttpResponseMessage> DeleteAsync(int id, string tableName)
        {
            if (Enum.TryParse(tableName, out Table table))
            {
                await baseService.DeleteAsync(id, table);
                return Request.CreateResponse(HttpStatusCode.OK, true);
            }

            return Request.CreateResponse(HttpStatusCode.BadRequest, false);
        }
    }

    public class UpdateRequest
    {
        public string TableName { get; set; }
        public string Field { get; set; }
        public string Value { get; set; } = "";
        public int Id { get; set; }
    }
}
