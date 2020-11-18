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
    public interface ICustomerRepository
    {
        Task<IEnumerable<Customer>> GetAllAsync();
    }

    public class CustomerRepository : ICustomerRepository
    {
        private readonly Func<IDbConnection> dbConnectionFactory;
        private static readonly string TABLE = "[ER.Customers]";
        private static readonly string[] FIELDS = typeof(Customer).DapperFields();

        public CustomerRepository(Func<IDbConnection> dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }


        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Customer>($"{DapperHelper.SELECT(TABLE, FIELDS)}")).ToArray();
            }
        }
    }
}
