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
        Task<Customer> GetAsync(Guid custId);
        Task<IEnumerable<Customer>> GetAllAsync(string filter = null);
        Task<(Customer newCustomer, bool Status)> RegisterAsync(Customer customer);
        Task<bool> UserDetailsExists(string field, string value);
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

        public async Task<Customer> GetAsync(Guid custId)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Customer>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE CustId = @custId", new { custId })).FirstOrDefault();
            }
        }

        public async Task<IEnumerable<Customer>> GetAllAsync(string filter = null)
        {
            using (var sql = dbConnectionFactory())
            {
                string sqlTxt = $@"
                    {DapperHelper.SELECT(TABLE, FIELDS)}
                    {(filter != null ? $@"WHERE
                        FirstName LIKE '%{filter}%' OR
                        LastName LIKE '%{filter}%' OR
                        Email LIKE '%{filter}%' OR
                        Address1 LIKE '%{filter}%' OR
                        Postcode LIKE '%{filter}%' OR
                        CustId LIKE '%{filter}%'" : "")}";

                return (await sql.QueryAsync<Customer>(sqlTxt)).ToArray();
            }
        }

        public async Task<bool> UserDetailsExists(string field, string value)
        {
            using (var sql = dbConnectionFactory())
            {
                return await sql.ExecuteScalarAsync<bool>($"SELECT count(1) FROM {TABLE} WHERE {field} = @Value", new { Value = value.Trim() });
            }
        }

        public async Task<(Customer newCustomer, bool Status)> RegisterAsync(Customer customer)
        {
            using (var sql = dbConnectionFactory())
            {
                try
                {
                    customer.CustId = Guid.NewGuid();
                    await sql.ExecuteAsync($"{DapperHelper.INSERT(TABLE, FIELDS)}", customer);

                    var newCustomer = await GetAsync(customer.CustId);
                    return (newCustomer, true);
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
