using DFM.Utils;
using Appology.ER.Model;
using Appology.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Transactions;
using Appology.Enums;
using Appology.Repository;

namespace Appology.ER.Repository
{
    public interface IOrderRepository
    {
        Task<(Order Order, bool Status)> GetAsync(Guid orderId);
        Task<IEnumerable<Order>> GetAllAsync(Guid customerId);
        Task<bool> OrderExists(Guid orderId);
        Task<(Order Order, Trip Trip, bool Status)> InsertOrUpdateAsync(Order order, Trip trip);
        Task<bool> DeleteOrder(Guid orderId);
        Task<bool> SetDeliveryDate(Guid orderId, DateTime date, string timeslot);
        Task<bool> UnsetDeliveryDate(Guid orderId);
        Task<bool> OrderPaid(Guid orderId, bool paid, string stripePaymentConfirmationId = null);
        Task<bool> OrderDispatch(Guid orderId, bool dispatch);
    }

    public class OrderRepository : DapperBaseRepository, IOrderRepository
    {
        private readonly ITripRepository tripRepository;
        private static readonly string TABLE = Tables.Name(Table.Orders);
        private static readonly string[] FIELDS = typeof(Order).DapperFields();

        public OrderRepository(Func<IDbConnection> dbConnectionFactory, ITripRepository tripRepository) : base(dbConnectionFactory)
        {
            this.tripRepository = tripRepository ?? throw new ArgumentNullException(nameof(tripRepository));
        }

        public async Task<(Order Order, bool Status)> GetAsync(Guid orderId)
        {
            string sqlTxt = $@"
                SELECT o.OrderId, o.ServiceId, c.Name AS ServiceName, o.Items, o.OrderValue, o.ServiceFee, o.OrderFee, o.DeliveryFee, o.TotalItems, o.Invoice, o.Created, o.Modified, o.DeliveryDate, o.Timeslot, o.Dispatched, o.Paid, o.StripePaymentConfirmationId
                FROM {TABLE} AS o
                LEFT JOIN {Tables.Name(Table.Categories)} AS c
                ON c.Id = o.ServiceId
                WHERE OrderId = '{orderId}'";

            var order = await QueryFirstOrDefaultAsync<Order>(sqlTxt);
            return (order, order != null);
        }

        public async Task<IEnumerable<Order>> GetAllAsync(Guid customerId)
        {
            return await QueryAsync<Order>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE CustomerId = @customerId", new { customerId });
        }

        public async Task<bool> OrderExists(Guid orderId)
        {
            return await ExecuteScalarAsync<bool>($"SELECT count(1) FROM {TABLE} WHERE OrderId = @orderId", new { orderId });
        }

        public async Task<(Order Order, Trip Trip, bool Status)> InsertOrUpdateAsync(Order order, Trip trip)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            { 
                order.Modified = DateUtils.FromTimeZoneToUtc(DateUtils.DateTime());

                if (!await OrderExists(order.OrderId))
                {
                    order.OrderId = Guid.NewGuid();
                    await ExecuteAsync($"{DapperHelper.INSERT(TABLE, FIELDS)}", order);
                }
                else
                {
                    await ExecuteAsync($"{DapperHelper.UPDATE(TABLE, FIELDS, "")} WHERE OrderId = @OrderId", order);
                }

                var newOrder = await GetAsync(order.OrderId);

                if (newOrder.Status)
                {
                    trip.OrderId = newOrder.Order.OrderId;
                    var orderTrip = await tripRepository.InsertOrUpdateAsync(trip);

                    if (orderTrip.Status)
                    {
                        scope.Complete();
                        return (newOrder.Order, orderTrip.Trip, true);
                    }
                    else
                    {
                        return (null, null, false);
                    }
                }
                else
                {
                    return (null, null, false);
                }
            }
        }

        public async Task<bool> DeleteOrder(Guid orderId)
        {
            await ExecuteAsync($@"{DapperHelper.DELETE(TABLE)} WHERE OrderId = @orderId", new { orderId });
            return await tripRepository.DeleteTripByOrderId(orderId);
        }

        public async Task<bool> SetDeliveryDate(Guid orderId, DateTime date, string timeslot)
        {
            return await ExecuteAsync($"UPDATE {TABLE} SET DeliveryDate = @date, Timeslot = @timeslot WHERE orderId = @orderId", 
                new {
                    orderId,
                    timeslot,
                    date
                });
        }

        public async Task<bool> UnsetDeliveryDate(Guid orderId)
        {
            return await ExecuteAsync($"UPDATE {TABLE} SET DeliveryDate = null, Timeslot = null WHERE orderId = @orderId",  new { orderId });
        }

        public async Task<bool> OrderPaid(Guid orderId, bool paid, string stripePaymentConfirmationId = null)
        {
            return await ExecuteAsync($"UPDATE {TABLE} SET Paid = @paid, StripePaymentConfirmationId = @stripePaymentConfirmationId WHERE OrderId = @orderId", 
                new { orderId, paid, stripePaymentConfirmationId });
        }
        
        public async Task<bool> OrderDispatch(Guid orderId, bool dispatch)
        {
            return await ExecuteAsync($"UPDATE {TABLE} SET Dispatched = @dispatch WHERE OrderId = @orderId", new { orderId, dispatch });
        }
    }
}
