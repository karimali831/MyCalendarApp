using Appology.Enums;
using Appology.MiCalendar.DTOs;
using Appology.MiCalendar.Model;
using Dapper;
using DFM.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Appology.MiCalendar.Repository
{
    public interface ITypeRepository
    {
        Task<IEnumerable<Types>> GetAllByUserIdAsync(Guid userId, TypeGroup? groupId = null);
        Task<IEnumerable<Types>> GetAllByGroupAsync(TypeGroup groupId);
        Task<Types> GetAsync(int Id);
        Task<bool> UpdateTypeAsync(Types type);
        Task<(bool Status, Types Calendar)> AddTypeAsync(TypeDTO type);
        Task<bool> DeleteTypeAsync(int Id);
        Task<bool> MoveTypeAsync(int Id, int? moveToId = null);
        Task<int[]> GetAllIdsByParentTypeIdAsync(int superTypeId);
        Task<bool> UpdateInvitees(string invitees, Guid userId);
    }

    public class TypeRepository : ITypeRepository
    {
        private readonly Func<IDbConnection> dbConnectionFactory;
        private static readonly string TABLE = Tables.Name(Table.Types);
        private static readonly string[] FIELDS = typeof(Types).DapperFields();
        private static readonly string[] DTOFIELDS = typeof(TypeDTO).DapperFields();

        public TypeRepository(Func<IDbConnection> dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }

        public async Task<IEnumerable<Types>> GetAllByUserIdAsync(Guid userId, TypeGroup? groupId)
        {
            using (var sql = dbConnectionFactory())
            {
                string sqlTxt = @$"
                    {DapperHelper.SELECT(TABLE, FIELDS)}
                    WHERE UserCreatedId = '{userId}'
                    {(groupId.HasValue ? $"AND GroupId = {(int)groupId.Value}" : "")}
                    ORDER BY SuperTypeId, Name ASC";

                return (await sql.QueryAsync<Types>(sqlTxt)).ToArray();
            }
        }

        public async Task<IEnumerable<Types>> GetAllByGroupAsync(TypeGroup groupId)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Types>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE GroupID = @groupId ORDER BY SuperTypeId, Name ASC", new { groupId })).ToArray();
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

        public async Task<(bool Status, Types Calendar)> AddTypeAsync(TypeDTO type)
        {
            using (var sql = dbConnectionFactory())
            {
                try
                {
                    var id = await sql.QuerySingleAsync<int>($"{DapperHelper.INSERT(TABLE, DTOFIELDS)}; SELECT CAST(SCOPE_IDENTITY() as int)", type);
                    var calendar = await GetAsync(id);

                    return (true, await GetAsync(id));
                }
                catch (Exception exp)
                {
                    string.IsNullOrEmpty(exp.Message);
                    return (false, null);
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

        public async Task<bool> MoveTypeAsync(int Id, int? moveToId = null)
        {
            using (var sql = dbConnectionFactory())
            {
                try
                {
                    await sql.ExecuteAsync($"UPDATE {TABLE} SET SuperTypeId = {(moveToId.HasValue ? moveToId : "null")} WHERE Id = @Id", new { Id });
                    return true;
                }
                catch (Exception exp)
                {
                    string.IsNullOrEmpty(exp.Message);
                    return false;
                }
            }
        }

        public async Task<int[]> GetAllIdsByParentTypeIdAsync(int superTypeId)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<int>($"SELECT Id FROM {TABLE} WHERE SuperTypeId = @superTypeId", new { superTypeId })).ToArray();
            }
        }

        public async Task<bool> UpdateInvitees(string invitees, Guid userId)
        {
            using (var sql = dbConnectionFactory())
            {
                try
                {
                    await sql.ExecuteAsync($"UPDATE {TABLE} SET InviteeIds = @invitees WHERE UserCreatedId = @userId", new { invitees, userId });
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
