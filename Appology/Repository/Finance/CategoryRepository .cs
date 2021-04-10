using Appology.Enums;
using Appology.MiFinance.DTOs;
using Appology.MiFinance.Model;
using Appology.Repository;
using DFM.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Appology.MiFinance.Repository
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task AddCategory(CategoryDTO dto);
        Task<string> GetCategoryName(int id);
        Task<int> GetSecondTypeId(int catId);
    }

    public class CategoryRepository : DapperBaseRepository, ICategoryRepository
    {
        private static readonly string TABLE = Tables.Name(Table.FinanceCategories);
        private static readonly string[] FIELDS = typeof(Category).DapperFields();
        private static readonly string[] DTOFIELDS = typeof(CategoryDTO).DapperFields();

        public CategoryRepository(Func<IDbConnection> dbConnectionFactory) : base(dbConnectionFactory) { }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await QueryAsync<Category>($"{DapperHelper.SELECT(TABLE, FIELDS)}");
        }

        public async Task<string> GetCategoryName(int id)
        {
            return await QueryFirstOrDefaultAsync<string>($"SELECT Name FROM {TABLE} WHERE Id = @Id", new { Id = id });
        }

        public async Task AddCategory(CategoryDTO dto)
        {
            await ExecuteAsync($@"{DapperHelper.INSERT(TABLE, DTOFIELDS)}", dto);
        }

        public async Task<int> GetSecondTypeId(int catId)
        {
            return await QueryFirstOrDefaultAsync<int>($"SELECT SecondTypeId FROM {TABLE} WHERE Id = @Id", new { Id = catId });
        }
    }
}
