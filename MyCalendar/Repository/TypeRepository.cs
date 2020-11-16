using Dapper;
using DFM.Utils;
using MyCalendar.DTOs;
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
        Task<IEnumerable<Types>> GetAllByUserIdAsync(Guid userId);
        Task<IEnumerable<Types>> GetAllAsync();
        Task<Types> GetAsync(int Id);
        Task<bool> UpdateTypeAsync(Types type);
        Task<bool> AddTypeAsync(TypeDTO type);
        Task<bool> DeleteTypeAsync(int Id);
    }

    public class TypeRepository : ITypeRepository
    {
        private readonly Func<IDbConnection> dbConnectionFactory;
        private static readonly string TABLE = "Types";
        private static readonly string[] FIELDS = typeof(Types).DapperFields();
        private static readonly string[] DTOFIELDS = typeof(TypeDTO).DapperFields();

        public TypeRepository(Func<IDbConnection> dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }

        public async Task<IEnumerable<Types>> GetAllByUserIdAsync(Guid userId)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Types>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE UserCreatedId = @userId ORDER BY SuperTypeId, Name ASC", new { userId })).ToArray();
            }
        }

        public async Task<IEnumerable<Types>> GetAllAsync()
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Types>($"{DapperHelper.SELECT(TABLE, FIELDS)} ORDER BY SuperTypeId, Name ASC")).ToArray();
            }
        }

        public async Task<Types> GetAsync(int Id)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Types>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE Id = @Id", new { Id })).FirstOrDefault();
            }
        }

        public async Task<bool> UpdateTypeAsync(Types type)
        {
            using (var sql = dbConnectionFactory())
            {
                try
                {
                    await sql.ExecuteAsync($"{DapperHelper.UPDATE(TABLE, DTOFIELDS, "")} WHERE Id = @Id", type);
                    return true;
                }
                catch (Exception exp)
                {
                    string.IsNullOrEmpty(exp.Message);
                    return false;
                }
            }
        }

        public async Task<bool> AddTypeAsync(TypeDTO type)
        {
            using (var sql = dbConnectionFactory())
            {
                try
                {
                    await sql.ExecuteAsync($"{DapperHelper.INSERT(TABLE, DTOFIELDS)}", type);
                    return true;
                }
                catch (Exception exp)
                {
                    string.IsNullOrEmpty(exp.Message);
                    return false;
                }
            }
        }

        public async Task<bool> DeleteTypeAsync(int Id)
        {
            using (var sql = dbConnectionFactory())
            {
                try
                {
                    await sql.ExecuteAsync($"{DapperHelper.DELETE(TABLE)} WHERE Id = @Id", new { Id } );
                    return true;
                }
                catch (Exception exp)
                {
                    string.IsNullOrEmpty(exp.Message);
                    return false;
                }
            }
        }
    }
}
