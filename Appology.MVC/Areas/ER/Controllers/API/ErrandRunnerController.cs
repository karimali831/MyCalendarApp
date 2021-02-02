using Appology.Controllers.Api;
using Appology.Enums;
using Appology.ER.Enums;
using Appology.ER.Model;
using Appology.ER.Service;
using Appology.Helpers;
using Appology.Service;
using Stripe;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace Appology.Areas.ER.Controllers.API
{
    [RoutePrefix("api/errandrunner")]
    [EnableCors(origins: "http://localhost:3000", headers: "*", methods: "*")]
    [CamelCaseControllerConfig]
    public class ErrandRunnerController : ApiController
    {
        private readonly IStakeholderService stakeholderService;
        private readonly ICategoryService categoryService;
        private readonly IOrderService orderService;
        private readonly ITripService tripService;

        public ErrandRunnerController(
            IStakeholderService stakeholderService, 
            ICategoryService categoryService,
            IOrderService orderService,
            ITripService tripService)
        {
            this.stakeholderService = stakeholderService ?? throw new ArgumentNullException(nameof(stakeholderService));
            this.categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            this.orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            this.tripService = tripService ?? throw new ArgumentNullException(nameof(tripService));
        }

        [Route("services")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetServices()
        {
            var services = await categoryService.GetAllAsync(Categories.ERServices, activeOnly: true);
            return Request.CreateResponse(HttpStatusCode.OK, new { services });
        }

        [Route("order/{orderId}")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetOrder(Guid orderId)
        {
            var order = await orderService.GetAsync(orderId);
            var trip = await tripService.GetByOrderIdAsync(orderId);

            if (orderId != Guid.Empty && order.Status && trip.Status)
            {
                var driver = await stakeholderService.GetAsync(trip.Trip.AssignedRunnerId);

                return Request.CreateResponse(HttpStatusCode.OK, new { 
                    order = order.Order, 
                    trip = trip.Trip,
                    driver = driver.Stakeholder, 
                    status = true 
                });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { status = false });
            }
        }

        [Route("orders/{userId}")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetOrders(Guid userId)
        {
            var orders = await orderService.GetAllAsync(userId);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                orders = orders.Select(x => new
                {
                    id = x.OrderId,
                    name = $"{x.Modified.ToShortDateString()} - {Utils.ToCurrency(x.Invoice)}"
                })
            });
        }

        [Route("saveorder")]
        [HttpPost]
        public async Task<HttpResponseMessage> SaveOrder(SaveOrderRequest request)
        {
            var order = await orderService.InsertOrUpdateAsync(request.Order, request.Trip);

            return Request.CreateResponse(HttpStatusCode.OK, new { 
                order.Order, 
                order.Trip, 
                order.Status 
            });
        }

        [Route("stakeholders/{stakeholderId}/{filter?}")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetStakeholders(Stakeholders stakeholderId, string filter = null)
        {
            var stakeholders = await stakeholderService.GetAllAsync(stakeholderId, filter);
            return Request.CreateResponse(HttpStatusCode.OK, new { Stakeholders = stakeholders });
        }

        [Route("stakeholders/register")]
        [HttpPost]
        public async Task<HttpResponseMessage> RegisterStakeholder(Stakeholder stakeholder)
        {
            var register = await stakeholderService.RegisterAsync(stakeholder);
            return Request.CreateResponse(HttpStatusCode.OK, new { Stakeholder = register.stakeholder, Message = register.Message });
        }
    }

    public class SaveOrderRequest
    {
        public Appology.ER.Model.Order Order { get; set; }
        public Trip Trip { get; set; }
    }
}
