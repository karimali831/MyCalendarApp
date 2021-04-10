using DFM.Utils;
using Appology.Enums;
using Appology.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Appology.Repository
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync(Categories parentId);
        Task<Category> GetAsync(int Id);
    }

    public class CategoryRepository : DapperBaseRepository, ICategoryRepository
    {
        private static readonly string TABLE = Tables.Name(Table.Categories);
        private static readonly string[] FIELDS = typeof(Category).DapperFields();

        public CategoryRepository(Func<IDbConnection> dbConnectionFactory) : base(dbConnectionFactory) { }

        public async Task<IEnumerable<Category>> GetAllAsync(Categories parentId)
        {
            return await QueryAsync<Category>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE ParentId = @parentId", new { parentId });
        }

        public async Task<Category> GetAsync(int Id)
        {
            return await QueryFirstOrDefaultAsync<Category>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE Id = @Id", new { Id });
        }

    }
}
