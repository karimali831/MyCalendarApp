using Appology.Controllers.Api;
using Appology.Enums;
using Appology.ER.Enums;
using Appology.ER.Model;
using Appology.ER.Service;
using Appology.Helpers;
using Appology.MiCalendar.Service;
using Appology.Model;
using Appology.Service;
using Stripe;
using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace Appology.Areas.ER.Controllers.API
{
    [RoutePrefix("api/errandrunner")]
    [CamelCaseControllerConfig]
    public class ErrandRunnerController : ApiController
    {
        private readonly IStakeholderService stakeholderService;
        private readonly ICategoryService categoryService;
        private readonly IOrderService orderService;
        private readonly ITripService tripService;
        private readonly IEventService eventService;
        private readonly IUserService userService;
        private readonly string rootUrl = ConfigurationManager.AppSettings["RootUrl"];

        public ErrandRunnerController(
            IStakeholderService stakeholderService, 
            ICategoryService categoryService,
            IOrderService orderService,
            ITripService tripService,
            IEventService eventService,
            IUserService userService)
        {
            this.stakeholderService = stakeholderService ?? throw new ArgumentNullException(nameof(stakeholderService));
            this.categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            this.orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            this.tripService = tripService ?? throw new ArgumentNullException(nameof(tripService));
            this.eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        private async Task<User> GetUser()
        {
            bool isLocal = this.rootUrl == "http://localhost:53822";
            return await userService.GetUser(isLocal ? "karimali831@googlemail.com" : null);
        }

        [Route("services")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetServices()
        {
            var services = await categoryService.GetAllAsync(Categories.ERServices, activeOnly: true);
            return Request.CreateResponse(HttpStatusCode.OK, new { services });
        }

        [Route("places")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetPlaces()
        {
            var places = (await orderService.GetAllPlacesAsync()).Where(x => x.DisplayController);
            return Request.CreateResponse(HttpStatusCode.OK, places);
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
                    name = $"{x.Modified.ToShortDateString()} - {Utils.ToCurrency(x.Invoice)} {(x.Paid ? "- Paid" : "")} {(x.Dispatched ? " & Dispatched" : "")}"
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

        [Route("deleteorder/{orderId}")]
        [HttpGet]
        public async Task<HttpResponseMessage> DeleteOrder(Guid orderId)
        {
            var order = await orderService.DeleteOrder(orderId);
            return Request.CreateResponse(HttpStatusCode.OK, order);
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

        [Route("setdeliverydate")]
        [HttpPost]
        public async Task<HttpResponseMessage> SetDeliveryDate(DeliveryDateRequest request)
        {
            var status = await orderService.SetDeliveryDate(request.OrderId, request.DeliveryDate, request.Timeslot);
            return Request.CreateResponse(HttpStatusCode.OK, status);
        }

        [Route("unsetdeliverydate/{orderId}")]
        [HttpGet]
        public async Task<HttpResponseMessage> UnsetDeliveryDate(Guid orderId)
        {
            var status = await orderService.UnsetDeliveryDate(orderId);
            return Request.CreateResponse(HttpStatusCode.OK, status);
        }

        [Route("orderpaid/{orderId}/{paid}/{stripePaymentConfirmationId?}")]
        [HttpGet]
        public async Task<HttpResponseMessage> OrderPaid(Guid orderId, bool paid, string stripePaymentConfirmationId = null)
        {
            var status = await orderService.OrderPaid(orderId, paid, stripePaymentConfirmationId);
            return Request.CreateResponse(HttpStatusCode.OK, status);
        }

        [Route("orderdispatch/{orderId}/{dispatch}")]
        [HttpGet]
        public async Task<HttpResponseMessage> OrderDispatch(Guid orderId, bool dispatch)
        {

            var order = await orderService.GetAsync(orderId);
            var user = await GetUser();

            var timeslot = order.Order.Timeslot.Split(' ');
            var startDateStr = string.Format("{0} {1}", order.Order.DeliveryDate.Value.ToString("dd/MM/yyyy"), timeslot[0]);
            var endDateStr = string.Format("{0} {1}", order.Order.DeliveryDate.Value.ToString("dd/MM/yyyy"), timeslot[2]);

            var startDate = DateUtils.FromTimeZoneToUtc(DateTime.ParseExact(startDateStr, "dd/MM/yyyy htt", CultureInfo.InvariantCulture));
            var endDate = DateUtils.FromTimeZoneToUtc(DateTime.ParseExact(endDateStr, "dd/MM/yyyy htt", CultureInfo.InvariantCulture));

            if (dispatch && order.Order != null && user != null) {

                var eventDTO = new Appology.MiCalendar.Model.Event
                {
                    CalendarId = 159,
                    UserID = user.UserID,
                    TagID = new Guid("290CDB32-2E5F-4E3B-80A5-4330466C0A09"),
                    StartDate = startDate,
                    EndDate = endDate,

                };

                await eventService.SaveEvent(eventDTO);
            }

            var status = await orderService.OrderDispatch(orderId, dispatch);
            return Request.CreateResponse(HttpStatusCode.OK, status);
        }
    }

    public class SaveOrderRequest
    {
        public Appology.ER.Model.Order Order { get; set; }
        public Trip Trip { get; set; }
    }

    public class DeliveryDateRequest
    {
        public Guid OrderId { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string Timeslot { get; set; }
    }
}
