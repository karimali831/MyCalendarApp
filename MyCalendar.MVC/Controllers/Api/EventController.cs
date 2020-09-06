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

    [RoutePrefix("api/events")]
    [CamelCaseControllerConfig]
    public class EventController : ApiController
    {
        private readonly IEventService eventService;

        public EventController(IEventService eventService)
        {
            this.eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
        }

        [Route("all")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetEventsAsync()
        {
            var events = await eventService.GetAllAsync();
            return Request.CreateResponse(HttpStatusCode.OK, new { Events = events });
        }

    }
}
