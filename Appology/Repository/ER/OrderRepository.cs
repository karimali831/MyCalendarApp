using Dapper;
using DFM.Utils;
using Appology.ER.Model;
using Appology.ER.Service;
using Appology.Helpers;
using Appology.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Appology.Enums;
using DFM.ExceptionHandling;
using DFM.ExceptionHandling.Sentry;
using System.Configuration;

namespace Appology.ER.Repository
{
    public interface IOrderRepository
    {
        Task<(Order Order, bool Status)> GetAsync(Guid orderId);
        Task<IEnumerable<Order>> GetAllAsync(Guid customerId);
        Task<bool> OrderExists(Guid orderId);
        Task<(Order Order, Trip Trip, bool Status)> InsertOrUpdateAsync(Order order, Trip trip);
        Task<bool> DeleteOrder(Guid orderId);
    }

    public class OrderRepository : IOrderRepository
    {
        private readonly Func<IDbConnection> dbConnectionFactory;
        private readonly ITripRepository tripRepository;
        private readonly IExceptionHandlerService exceptionHandlerService;
        private static readonly string TABLE = Tables.Name(Table.Orders);
        private static readonly string[] FIELDS = typeof(Order).DapperFields();

        public OrderRepository(Func<IDbConnection> dbConnectionFactory, ITripRepository tripRepository)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
            this.tripRepository = tripRepository ?? throw new ArgumentNullException(nameof(tripRepository));
            this.exceptionHandlerService = new ExceptionHandlerService(ConfigurationManager.AppSettings["DFM.ExceptionHandling.Sentry.Environment"]);
        }

        public async Task<(Order Order, bool Status)> GetAsync(Guid orderId)
        {
            using (var sql = dbConnectionFactory())
            {
                try
                {
                    string sqlTxt = $@"
                    SELECT o.OrderId, o.ServiceId, c.Name AS ServiceName, o.Items, o.OrderValue, o.ServiceFee, o.OrderFee, o.DeliveryFee, o.TotalItems, o.Invoice, o.Created, o.Modified
                    FROM {TABLE} AS o
                    LEFT JOIN {Tables.Name(Table.Categories)} AS c
                    ON c.Id = o.ServiceId
                    WHERE OrderId = '{orderId}'";

                    var order = (await sql.QueryAsync<Order>(sqlTxt)).FirstOrDefault();
                    return (order, true);
                }
                catch (Exception exp)
                {
                    exceptionHandlerService.ReportException(exp).Submit();
                    return (null, false);
                }
            }
        }

        public async Task<IEnumerable<Order>> GetAllAsync(Guid customerId)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Order>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE CustomerId = @customerId", new { customerId })).ToArray();
            }
        }

        public async Task<bool> OrderExists(Guid orderId)
        {
            using (var sql = dbConnectionFactory())
            {
                return await sql.ExecuteScalarAsync<bool>($"SELECT count(1) FROM {TABLE} WHERE OrderId = @orderId", new { orderId });
            }
        }

        public async Task<(Order Order, Trip Trip, bool Status)> InsertOrUpdateAsync(Order order, Trip trip)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            using (var sql = dbConnectionFactory())
            {
                try
                {
                    order.Modified = DateUtils.FromTimeZoneToUtc(DateUtils.DateTime());

                    if (!await OrderExists(order.OrderId))
                    {
                        order.OrderId = Guid.NewGuid();
                        await sql.ExecuteAsync($"{DapperHelper.INSERT(TABLE, FIELDS)}", order);
                    }
                    else
                    {
                        await sql.ExecuteAsync($"{DapperHelper.UPDATE(TABLE, FIELDS, "")} WHERE OrderId = @OrderId", order);
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
                catch (Exception exp)
                {
                    exceptionHandlerService.ReportException(exp).Submit();
                    return (null, null, false);
                }
            }
        }

        public async Task<bool> DeleteOrder(Guid orderId)
        {
            try
            {
                using var sql = dbConnectionFactory();
                await sql.ExecuteAsync($@"{DapperHelper.DELETE(TABLE)} WHERE OrderId = @orderId", new { orderId });

                return await tripRepository.DeleteTripByOrderId(orderId);
            }
            catch (Exception exp)
            {
                exceptionHandlerService.ReportException(exp).Submit();
                return false;
            }
        }
    }
}
