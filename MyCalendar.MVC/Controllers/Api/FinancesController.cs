using MyCalendar.Service;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace MyCalendar.Website.Controllers.API
{

    public class CamelCaseControllerConfigAttribute : Attribute, IControllerConfiguration
    {
        public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            var formatter = controllerSettings.Formatters.OfType<JsonMediaTypeFormatter>().Single();
            controllerSettings.Formatters.Remove(formatter);

            formatter = new JsonMediaTypeFormatter
            {
                SerializerSettings = { ContractResolver = new CamelCasePropertyNamesContractResolver() }
            };

            controllerSettings.Formatters.Add(formatter);
        }
    }

    [RoutePrefix("api/Calendar")]
    [CamelCaseControllerConfig]
    public class CalendarController : ApiController
    {
        private readonly ICalendarService CalendarService;

        public CalendarController(
            ICalendarService CalendarService)
        {
            this.CalendarService = CalendarService ?? throw new ArgumentNullException(nameof(CalendarService));
        }

        [Route("all")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetCalendarAsync()
        {
            var Calendar = await CalendarService.GetAllAsync();

            return Request.CreateResponse(HttpStatusCode.OK, new { Calendar = Calendar });
        }

    }
}
