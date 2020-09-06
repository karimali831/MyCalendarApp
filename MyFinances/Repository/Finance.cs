using Dapper;
using DFM.Utils;
using MyFinances.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace MyFinances.Repository
{
    public interface IFinanceRepository
    {
        Task<IEnumerable<Finance>> GetAllAsync();
    }

    public class FinanceRepository : IFinanceRepository
    {
        private readonly Func<IDbConnection> dbConnectionFactory;
        private static readonly string TABLE = "Finances";
        private static readonly string[] FIELDS = typeof(Finance).DapperFields();

        public FinanceRepository(Func<IDbConnection> dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }

        public async Task<IEnumerable<Finance>> GetAllAsync()
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Finance>($"{DapperHelper.SELECT(TABLE, FIELDS)}")).ToArray();
            }
        }
    }
}
