using MyCalendar.Enums;
using MyCalendar.ER.Model;
using MyCalendar.ER.Service;
using MyCalendar.Service;
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
        private readonly ICategoryService categoryService;

        public ErrandRunnerController(ICustomerService customerService, ICategoryService categoryService)
        {
            this.customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            this.categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
        }

        [Route("services")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetServices()
        {
            var services = await categoryService.GetAllAsync(Categories.ERServices, activeOnly: true);
            return Request.CreateResponse(HttpStatusCode.OK, new { services });
        }

        [Route("customers/{filter}")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetCustomers(string filter)
        {
            var customers = await customerService.GetAllAsync(filter);
            return Request.CreateResponse(HttpStatusCode.OK, new { Customers = customers });
        }

        [Route("customers/register")]
        [HttpPost]
        public async Task<HttpResponseMessage> RegisterCustomer(Customer customer)
        {
            var register = await customerService.RegisterAsync(customer);
            return Request.CreateResponse(HttpStatusCode.OK, new { Customer = register.customer, Message = register.Message });
        }

    }
}
