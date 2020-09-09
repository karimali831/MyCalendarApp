using Dapper;
using DFM.Utils;
using MyCalendar.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace MyCalendar.Repository
{
    public interface ITypeRepository
    {
        Task<IEnumerable<Types>> GetAllAsync();
    }

    public class TypeRepository : ITypeRepository
    {
        private readonly Func<IDbConnection> dbConnectionFactory;
        private static readonly string TABLE = "Types";
        private static readonly string[] FIELDS = typeof(Types).DapperFields();

        public TypeRepository(Func<IDbConnection> dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }


        public async Task<IEnumerable<Types>> GetAllAsync()
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Types>($"{DapperHelper.SELECT(TABLE, FIELDS)}")).ToArray();
            }
        }

    }
}
