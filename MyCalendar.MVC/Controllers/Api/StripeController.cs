using MyCalendar.Enums;
using MyCalendar.ER.Model;
using MyCalendar.ER.Service;
using MyCalendar.Helpers;
using MyCalendar.Model;
using MyCalendar.Service;
using MyCalendar.Website.Controllers.Api;
using Newtonsoft.Json;
using Stripe;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using Order = MyCalendar.ER.Model.Order;

namespace MyCalendar.Website.Controllers.API
{
    [RoutePrefix("api/stripe")]
    [EnableCors(origins: "http://localhost:3000", headers: "*", methods: "*")]
    [CamelCaseControllerConfig]
    public class StripeController : ApiController
    {
        private readonly IStakeholderService stakeholderService;
        private readonly IOrderService orderService;
        //private readonly string stripeApiKey;

        public StripeController(IStakeholderService stakeholderService, IOrderService orderService)
        {
            this.stakeholderService = stakeholderService ?? throw new ArgumentNullException(nameof(stakeholderService));
            this.orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));

            StripeConfiguration.ApiKey = "sk_test_51HlxO9BR6iw4dg3G50oyD9TTVmceaE82xx2lsi1GFaGFEcvBlixMXJjgAkaxOoEWPBiOOYSQrERNZFjYxeFeZkD700Jkahfqzu";
            //stripeApiKey = StripeConfiguration.ApiKey;

        }


        //[Route("payment")]
        //[HttpPost]
        //public async Task<HttpResponseMessage> StripePayment(PaymentChargeRequest request)
        //{
        //    var status = false;
        //    var customer = await stakeholderService.GetAsync(request.CustomerId);

        //    var options = new ChargeCreateOptions
        //    {
        //        Amount = 099,
        //        Currency = "gbp",
        //        Description = "Test",
        //        Source = new CardCreateNestedOptions
        //        {
        //            Number = request.Number,
        //            Cvc = request.Cvc,
        //            ExpMonth = request.ExpMonth,
        //            ExpYear = request.ExpYear
        //        },
        //        Customer = request.CustomerId.ToString(),

        //    };

        //    var service = new ChargeService();
        //    var charge = service.Create(options);

        //    if (charge != null && charge.Paid)
        //    {
        //        status = true;

        //        customer.Stakeholder.PaymentIdsList.Add(charge.Id);
        //        await stakeholderService.UpdatePaymentIds(customer.Stakeholder.PaymentIdsList, request.CustomerId);

        //    }

        //    return Request.CreateResponse(HttpStatusCode.OK, status);
        //}

        [Route("create-payment-intent")]
        [HttpPost]
        public async Task<HttpResponseMessage> Create(PaymentIntentCreateRequest request)
        {
            var order = await orderService.GetAsync(request.OrderId);

            if (order.Status && order.Order != null && VerifyInvoiceAmount(order.Order, request.Invoice))
            {

                var paymentIntents = new PaymentIntentService();
                var paymentIntent = paymentIntents.Create(new PaymentIntentCreateOptions
                {
                    Amount = ((int)order.Order.Invoice) * 100,
                    Currency = "gbp",
                });

                return Request.CreateResponse(HttpStatusCode.OK, new { clientSecret = paymentIntent.ClientSecret, status = true });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { clientSecret = "", status = false});
            }
        }

        private bool VerifyInvoiceAmount(Order order, decimal invoice)
        {
            var items = JsonConvert.DeserializeObject<IEnumerable<OrderItems>>(order.Items);
            decimal orderValue = items.Sum(x => x.Cost * x.Qty);
            decimal calcInvoice = orderValue + order.ServiceFee + order.OrderFee + order.DeliveryFee;

            return calcInvoice == order.Invoice && order.Invoice == invoice;
        }
    }


    public class PaymentIntentCreateRequest
    {
        public Guid OrderId { get; set; }
        public decimal Invoice { get; set; }
    }
}
