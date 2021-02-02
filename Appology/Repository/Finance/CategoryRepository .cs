using Appology.Enums;
using Appology.MiFinance.DTOs;
using Appology.MiFinance.Model;
using Dapper;
using DFM.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

    public class CategoryRepository : ICategoryRepository
    {
        private readonly Func<IDbConnection> dbConnectionFactory;
        private static readonly string TABLE = Tables.Name(Table.FinanceCategories);
        private static readonly string[] FIELDS = typeof(Category).DapperFields();
        private static readonly string[] DTOFIELDS = typeof(CategoryDTO).DapperFields();

        public CategoryRepository(Func<IDbConnection> dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            using var sql = dbConnectionFactory();
            return (await sql.QueryAsync<Category>($"{DapperHelper.SELECT(TABLE, FIELDS)}")).ToArray();
        }

        public async Task<string> GetCategoryName(int id)
        {
            using var sql = dbConnectionFactory();
            return (await sql.QueryAsync<string>($"SELECT Name FROM {TABLE} WHERE Id = @Id", new { Id = id })).FirstOrDefault();
        }

        public async Task AddCategory(CategoryDTO dto)
        {
            using var sql = dbConnectionFactory();
            await sql.ExecuteAsync($@"{DapperHelper.INSERT(TABLE, DTOFIELDS)}", dto);
        }

        public async Task<int> GetSecondTypeId(int catId)
        {
            using var sql = dbConnectionFactory();
            return (await sql.QueryAsync<int>($"SELECT SecondTypeId FROM {TABLE} WHERE Id = @Id", new { Id = catId })).FirstOrDefault();
        }
    }
}
