using Dapper;
using DFM.Utils;
using MyCalendar.Enums;
using MyCalendar.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace MyCalendar.Repository
{
    public interface IRoleRepository
    {
        Task<IEnumerable<Role>> GetAllAsync();
        Task<Role> GetAsync(Guid Id);
    }

    public class RoleRepository : IRoleRepository
    {
        private readonly Func<IDbConnection> dbConnectionFactory;
        private static readonly string TABLE = "Roles";
        private static readonly string[] FIELDS = typeof(Role).DapperFields();

        public RoleRepository(Func<IDbConnection> dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Role>($"{DapperHelper.SELECT(TABLE, FIELDS)}")).ToArray();
            }
        }

        public async Task<Role> GetAsync(Guid Id)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Role>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE Id = @Id", new { Id })).FirstOrDefault();
            }
        }

    }
}
