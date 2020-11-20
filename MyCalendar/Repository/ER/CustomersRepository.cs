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
        Task<IEnumerable<Customer>> GetAllAsync(string filter = null);
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


        public async Task<IEnumerable<Customer>> GetAllAsync(string filter = null)
        {
            using (var sql = dbConnectionFactory())
            {
                string sqlTxt = $@"
                    {DapperHelper.SELECT(TABLE, FIELDS)}
                    {(filter != null ? $@"WHERE
                        FirstName LIKE '%{filter}%' OR
                        LastName LIKE '%{filter}%' OR
                        Address1 LIKE '%{filter}%' OR
                        Postcode LIKE '%{filter}%' OR
                        CustId LIKE '%{filter}%'" : "")}";

                return (await sql.QueryAsync<Customer>(sqlTxt)).ToArray();
            }
        }

    }
}
