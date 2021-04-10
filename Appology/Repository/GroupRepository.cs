using DFM.Utils;
using Appology.Enums;
using Appology.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Appology.Repository
{
    public interface IGroupRepository
    {
        Task<IEnumerable<Group>> GetAllAsync();
        Task<Group> GetAsync(int Id);
    }

    public class GroupRepository : DapperBaseRepository, IGroupRepository
    {
        private static readonly string TABLE = Tables.Name(Table.Groups);
        private static readonly string[] FIELDS = typeof(Group).DapperFields();

        public GroupRepository(Func<IDbConnection> dbConnectionFactory) : base(dbConnectionFactory) { }

        public async Task<IEnumerable<Group>> GetAllAsync()
        {
            return await QueryAsync<Group>($"{DapperHelper.SELECT(TABLE, FIELDS)}");
        }

        public async Task<Group> GetAsync(int Id)
        {
            return await QueryFirstOrDefaultAsync<Group>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE Id = @Id", new { Id });
        }

    }
}
