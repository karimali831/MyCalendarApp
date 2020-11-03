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
    public interface ITypeRepository
    {
        Task<IEnumerable<Types>> GetSuperTypesAsync();
        Task<IEnumerable<Types>> GetAllByUserIdAsync(Guid userId);
        Task<Types> GetAsync(int Id);
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


        public async Task<IEnumerable<Types>> GetSuperTypesAsync()
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Types>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE SuperTypeId IS NULL")).ToArray();
            }
        }

        public async Task<IEnumerable<Types>> GetAllByUserIdAsync(Guid userId)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Types>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE UserCreatedId = @userId", new { userId })).ToArray();
            }
        }

        public async Task<Types> GetAsync(int Id)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Types>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE Id = @Id", new { Id })).FirstOrDefault();
            }
        }

    }
}
