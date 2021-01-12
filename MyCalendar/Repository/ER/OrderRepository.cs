using Dapper;
using DFM.Utils;
using MyCalendar.ER.Model;
using MyCalendar.Helpers;
using MyCalendar.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace MyCalendar.ER.Repository
{
    public interface IOrderRepository
    {
        Task<Order> GetAsync(Guid orderId);
        Task<bool> OrderExists(Guid orderId);
        Task<(Order Order, bool Status)> InsertOrUpdateAsync(Order order);
    }

    public class OrderRepository : IOrderRepository
    {
        private readonly Func<IDbConnection> dbConnectionFactory;
        private static readonly string TABLE = "[ER.Orders]";
        private static readonly string[] FIELDS = typeof(Order).DapperFields();

        public OrderRepository(Func<IDbConnection> dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }

        public async Task<Order> GetAsync(Guid orderId)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Order>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE OrderId = @orderId", new { orderId })).FirstOrDefault();
            }
        }

        public async Task<bool> OrderExists(Guid orderId)
        {
            using (var sql = dbConnectionFactory())
            {
                return await sql.ExecuteScalarAsync<bool>($"SELECT count(1) FROM {TABLE} WHERE OrderId = @orderId", new { orderId });
            }
        }

        public async Task<(Order Order, bool Status)> InsertOrUpdateAsync(Order order)
        {
            using (var sql = dbConnectionFactory())
            {
                try
                {
                    if (!await OrderExists(order.OrderId))
                    {
                        order.OrderId = Guid.NewGuid();
                        await sql.ExecuteAsync($"{DapperHelper.INSERT(TABLE, FIELDS)}", order);
                    }
                    else
                    {
                        order.Modified = Utils.FromTimeZoneToUtc(Utils.DateTime());
                        await sql.ExecuteAsync($"{DapperHelper.UPDATE(TABLE, FIELDS, "")} WHERE OrderId = @OrderId", order);
                    }

                    return (await GetAsync(order.OrderId), true);
                }
                catch (Exception exp)
                {
                    string.IsNullOrEmpty(exp.Message);
                    return (null, false);
                }
            }
        }
    }
}
