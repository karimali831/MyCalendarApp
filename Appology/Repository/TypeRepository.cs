using Appology.DTOs;
using Appology.Enums;
using Appology.Model;
using DFM.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Appology.Repository
{
    public interface ITypeRepository
    {
        Task<IEnumerable<Types>> GetAllByUserIdAsync(Guid userId, TypeGroup? groupId = null, bool userCreatedOnly = true);
        Task<IEnumerable<Types>> GetAllByGroupAsync(TypeGroup groupId);
        Task<Types> GetAsync(int Id);
        Task<bool> UpdateTypeAsync(Types type);
        Task<(bool Status, Types Calendar)> AddTypeAsync(TypeDTO type);
        Task<bool> DeleteTypeAsync(int Id);
        Task<bool> MoveTypeAsync(int Id, int? moveToId = null);
        Task<int[]> GetAllIdsByParentTypeIdAsync(int superTypeId);
        Task<bool> UpdateInvitees(string invitees, Guid userId);
    }

    public class TypeRepository : DapperBaseRepository, ITypeRepository
    {
        private static readonly string TABLE = Tables.Name(Table.Types);
        private static readonly string[] FIELDS = typeof(Types).DapperFields();
        private static readonly string[] DTOFIELDS = typeof(TypeDTO).DapperFields();

        public TypeRepository(Func<IDbConnection> dbConnectionFactory) : base(dbConnectionFactory) { }

        public async Task<IEnumerable<Types>> GetAllByUserIdAsync(Guid userId, TypeGroup? groupId, bool userCreatedOnly = true)
        {
            string sqlTxt = @$"
                {DapperHelper.SELECT(TABLE, FIELDS)}
                WHERE (UserCreatedId = '{userId}'
                {(!userCreatedOnly ? $"OR (',' + RTRIM(InviteeIds) + ',') LIKE '%,{userId},%')" : ")")}
                {(groupId.HasValue ? $"AND GroupId = {(int)groupId.Value}" : "")}
                ORDER BY SuperTypeId, Name ASC";

            return await QueryAsync<Types>(sqlTxt);
        }

        public async Task<IEnumerable<Types>> GetAllByGroupAsync(TypeGroup groupId)
        {
            return await QueryAsync<Types>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE GroupID = @groupId ORDER BY SuperTypeId, Name ASC", new { groupId });
        }

        public async Task<Types> GetAsync(int Id)
        {
            return await QueryFirstOrDefaultAsync<Types>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE Id = @Id", new { Id });
        }

        public async Task<bool> UpdateTypeAsync(Types type)
        {
            return await ExecuteAsync($"{DapperHelper.UPDATE(TABLE, DTOFIELDS, "")} WHERE Id = @Id", type);
        }

        public async Task<(bool Status, Types Calendar)> AddTypeAsync(TypeDTO type)
        {
            var id = await QuerySingleAsync<int>($"{DapperHelper.INSERT(TABLE, DTOFIELDS)}; SELECT CAST(SCOPE_IDENTITY() as int)", type);
            return (true, await GetAsync(id));
        }

        public async Task<bool> DeleteTypeAsync(int Id)
        {
            return await ExecuteAsync($"{DapperHelper.DELETE(TABLE)} WHERE Id = @Id", new { Id } );
        }

        public async Task<bool> MoveTypeAsync(int Id, int? moveToId = null)
        {
             return await ExecuteAsync($"UPDATE {TABLE} SET SuperTypeId = {(moveToId.HasValue ? moveToId : "null")} WHERE Id = @Id", new { Id });
        }

        public async Task<int[]> GetAllIdsByParentTypeIdAsync(int superTypeId)
        {
            return (await QueryAsync<int>($"SELECT Id FROM {TABLE} WHERE SuperTypeId = @superTypeId", new { superTypeId })).ToArray();
        }

        public async Task<bool> UpdateInvitees(string invitees, Guid userId)
        {
            return await ExecuteAsync($"UPDATE {TABLE} SET InviteeIds = @invitees WHERE UserCreatedId = @userId", new { invitees, userId });
        }
    }
}
