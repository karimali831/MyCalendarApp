using Appology.Controllers.Api;
using Appology.ER.Model;
using Appology.ER.Service;
using Appology.Helpers;
using Newtonsoft.Json;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using Order = Appology.ER.Model.Order;

namespace Appology.Areas.ER.Controllers.API
{
    [RoutePrefix("api/stripe")]
    [CamelCaseControllerConfig]
    public class StripeController : ApiController
    {
        private readonly IStakeholderService stakeholderService;
        private readonly IOrderService orderService;

        public StripeController(IStakeholderService stakeholderService, IOrderService orderService)
        {
            this.stakeholderService = stakeholderService ?? throw new ArgumentNullException(nameof(stakeholderService));
            this.orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));

            StripeConfiguration.ApiKey = "sk_test_51HlxO9BR6iw4dg3G50oyD9TTVmceaE82xx2lsi1GFaGFEcvBlixMXJjgAkaxOoEWPBiOOYSQrERNZFjYxeFeZkD700Jkahfqzu";
        }


        [Route("create-payment-intent")]
        [HttpPost]
        public async Task<HttpResponseMessage> Create(PaymentIntentCreateRequest request)
        {
            var order = await orderService.GetAsync(request.OrderId);
            var verify = VerifyInvoiceAmount(order.Order, request.Invoice);

            if (order.Status && order.Order != null && verify == true)
            {

                var paymentIntents = new PaymentIntentService();

                var paymentIntent = paymentIntents.Create(new PaymentIntentCreateOptions
                {
                    Amount = ((int)order.Order.Invoice) * 100,
                    Currency = "gbp",
                    Metadata = new Dictionary<string, string>
                    {
                        {"OrderId", order.Order.OrderId.ToString()}
                    }
                });

                return Request.CreateResponse(HttpStatusCode.OK, new { clientSecret = paymentIntent.ClientSecret, status = true });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { clientSecret = "", status = false });
            }
        }

        private bool VerifyInvoiceAmount(Order order, decimal invoice)
        {
            try
            {
                var items = JsonConvert.DeserializeObject<IEnumerable<OrderItems>>(order.Items);

                decimal orderValue = items.Sum(x => x.Cost * x.Qty);
                decimal calcInvoice = Utils.ToMoney(orderValue + order.ServiceFee + order.OrderFee + order.DeliveryFee);
                decimal orderInvoice = Utils.ToMoney(order.Invoice);

                return calcInvoice == orderInvoice && orderInvoice == Utils.ToMoney(invoice);
            }
            catch
            {
                return false;
            }
        }
    }

    public class PaymentIntentCreateRequest
    {
        public Guid OrderId { get; set; }
        public decimal Invoice { get; set; }
    }
}
