using MyCalendar.ER.Service;
using MyCalendar.Website.Controllers.Api;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace MyCalendar.Website.Controllers.API
{
    [RoutePrefix("api/errandrunner")]
    [EnableCors(origins: "http://localhost:3000", headers: "*", methods: "*")]
    [CamelCaseControllerConfig]
    public class ErrandRunnerController : ApiController
    {
        private readonly ICustomerService customerService;

        public ErrandRunnerController(ICustomerService customerService)
        {
            this.customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        }

        [Route("customers")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetCustomers()
        {
            var customers = await customerService.GetAllAsync();
            return Request.CreateResponse(HttpStatusCode.OK, new { customers });
        }

    }
}
