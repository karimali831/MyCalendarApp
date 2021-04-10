using DFM.Utils;
using Appology.Enums;
using Appology.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Appology.Repository
{
    public interface IRoleRepository
    {
        Task<IEnumerable<Role>> GetAllAsync();
        Task<Role> GetAsync(Guid Id);
    }

    public class RoleRepository : DapperBaseRepository, IRoleRepository
    {
        private static readonly string TABLE = Tables.Name(Table.Roles);
        private static readonly string[] FIELDS = typeof(Role).DapperFields();

        public RoleRepository(Func<IDbConnection> dbConnectionFactory) : base(dbConnectionFactory) { }

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            return await QueryAsync<Role>($"{DapperHelper.SELECT(TABLE, FIELDS)}");
        }

        public async Task<Role> GetAsync(Guid Id)
        {
            return await QueryFirstOrDefaultAsync<Role>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE Id = @Id", new { Id });
        }
    }
}
