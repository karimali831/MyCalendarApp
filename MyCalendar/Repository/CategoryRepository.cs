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
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync(Categories parentId);
        Task<Category> GetAsync(int Id);
    }

    public class CategoryRepository : ICategoryRepository
    {
        private readonly Func<IDbConnection> dbConnectionFactory;
        private static readonly string TABLE = "Categories";
        private static readonly string[] FIELDS = typeof(Category).DapperFields();

        public CategoryRepository(Func<IDbConnection> dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }

        public async Task<IEnumerable<Category>> GetAllAsync(Categories parentId)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Category>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE ParentId = @parentId", new { parentId })).ToArray();
            }
        }

        public async Task<Category> GetAsync(int Id)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Category>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE Id = @Id", new { Id })).FirstOrDefault();
            }
        }

    }
}
