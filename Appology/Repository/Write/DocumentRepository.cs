using DFM.Utils;
using Appology.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Appology.Enums;
using Appology.Write.Model;
using Appology.Write.ViewModels;
using Appology.Write.DTOs;
using Appology.Repository;

namespace Appology.Write.Repository
{
    public interface IDocumentRepository
    {
        Task<IEnumerable<Document>> GetAllByTypeIdAsync(int typeId);
        Task<Document> GetAsync(Guid Id, Guid userId);
        Task<bool> InsertOrUpdateAsync(DocumentDTO dto);
        Task<bool> MoveAsync(Guid docId, int moveToId);
        Task<bool> DocumentsExistsInGroup(int groupId);
        Task<bool> DeleteDocument(Guid docId);
        Task<DocumentTitlesVM> GetDocumentTitle(Guid docId);
        Task<IList<DocumentTitlesVM>> GetDocumentTitles(Guid userId);
        Task<IList<DocumentTitlesVM>> SearchDocumentsByFilter(IEnumerable<Guid> docIds, string filter, Guid userId);
        Task<IEnumerable<(Guid DocId, string Tag)>> GetAllDocumentUserTags(Guid userId);
        Task<bool> DocumentExists(Guid docId, Guid userCreatedId);
    }

    public class DocumentRepository : DapperBaseRepository, IDocumentRepository
    {
        private static readonly string TABLE = Tables.Name(Table.Documents);
        private static readonly string[] FIELDS = typeof(Document).DapperFields();

        public DocumentRepository(Func<IDbConnection> dbConnectionFactory) : base(dbConnectionFactory) { }

        public async Task<IList<DocumentTitlesVM>> GetDocumentTitles(Guid userId)
        {
            string sqlTxt = $@"
                SELECT d.Id, d.TypeId, d.Title, d.EditedDate, d.EditedById, d.EditedAuto, d.Tags, d.UserCreatedId
                FROM {TABLE} AS d
                LEFT JOIN {Tables.Name(Table.Types)} AS t
                ON d.TypeId = t.Id
                WHERE  t.UserCreatedId = '{userId}'
                OR (',' + RTRIM(t.InviteeIds) + ',') LIKE '%,{userId},%'
                ORDER BY EditedDate DESC";

            return await QueryAsync<DocumentTitlesVM>(sqlTxt);
        }

        public async Task<DocumentTitlesVM> GetDocumentTitle(Guid docId)
        {
            return await QueryFirstOrDefaultAsync<DocumentTitlesVM>($"SELECT Id, TypeId, Title, EditedDate, EditedById, EditedAuto, Tags, UserCreatedId FROM {TABLE} WHERE Id = @docId", new { docId });
        }

        public async Task<IList<DocumentTitlesVM>> SearchDocumentsByFilter(IEnumerable<Guid> docIds, string filter, Guid userId)
        {
            string sqlTxt = $@"
                SELECT d.Id, d.Title, EditedAuto
                FROM {TABLE} AS d
                LEFT JOIN {Tables.Name(Table.Types)} AS t
                ON d.TypeId = t.Id
                WHERE (d.Title LIKE '%{filter}%' OR d.ID IN ('{string.Join(",",docIds)}'))
                AND (t.UserCreatedId = '{userId}'
                OR (',' + RTRIM(t.InviteeIds) + ',') LIKE '%,{userId},%')
                ORDER BY EditedDate DESC";

            return await QueryAsync<DocumentTitlesVM>(sqlTxt);
        }

        public async Task<IEnumerable<Document>> GetAllByTypeIdAsync(int typeId)
        {
            return await QueryAsync<Document>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE TypeId = @typeId", new { typeId });
        }

        public async Task<IEnumerable<(Guid DocId, string Tag)>> GetAllDocumentUserTags(Guid userId)
        {

            return await QueryAsync<(Guid, string)>($"SELECT Id, Tags FROM {TABLE} WHERE UserCreatedId = @userId AND datalength(tags)!=0", new { userId });
            
        }

        public async Task<bool> DeleteDocument(Guid docId)
        {
            var deleteDoc = await ExecuteAsync($"{DapperHelper.DELETE(TABLE)} WHERE Id = @docId", new { docId });
            var docDocChanges = await ExecuteAsync($"{DapperHelper.DELETE(Tables.Name(Table.DocumentChangelog))} WHERE DocId = @docId", new { docId });

            return deleteDoc && docDocChanges;
        }

        public async Task<Document> GetAsync(Guid Id, Guid userId)
        {
            string sqlTxt = $@"
                SELECT d.Id, d.TypeId, d.Title, d.Text, d.DraftText, d.CreatedDate, d.EditedDate, d.UserCreatedId, d.EditedById, d.EditedAuto, d.Tags
                FROM {TABLE} AS d
                LEFT JOIN {Tables.Name(Table.Types)} AS t
                ON d.TypeId = t.Id
                WHERE d.Id = '{Id}'
                AND (t.UserCreatedId = '{userId}'
                OR (',' + RTRIM(t.InviteeIds) + ',') LIKE '%,{userId},%')";

            return await QueryFirstOrDefaultAsync<Document>(sqlTxt);
        }

        public async Task<bool> DocumentExists(Guid docId, Guid userCreatedId)
        {
            return await ExecuteScalarAsync<bool>($"SELECT count(1) FROM {TABLE} WHERE Id = @docId AND UserCreatedId = @userCreatedId", new { docId, userCreatedId });
        }

        public async Task<bool> MoveAsync(Guid docId, int moveToId)
        {
            return await ExecuteAsync($"UPDATE {TABLE} SET TypeId = @moveToId WHERE Id = @docId", new { docId, moveToId });
        }

        public async Task<bool> InsertOrUpdateAsync(DocumentDTO dto)
        {
            Func<DocumentDTO, object> saveDocument = (DocumentDTO d) =>
                new
                {
                    id = d.Id,
                    typeId = d.TypeId,
                    title = d.Title,
                    draftText = d.DraftText,
                    text = d.Text,
                    userCreatedId = d.UserCreatedId,
                    editedDate = DateUtils.FromTimeZoneToUtc(DateUtils.DateTime()),
                    editedById = d.EditedById,
                    createdDate = d.CreatedDate,
                    editedAuto = d.EditedAuto,
                    tags = d.TagsList != null && d.TagsList.Any() ? string.Join(",", d.TagsList) : null
                };

            var existing = await DocumentExists(dto.Id.Value, dto.UserCreatedId);

            if (existing == false)
            {
                await ExecuteAsync($"{DapperHelper.INSERT(TABLE, FIELDS)}", saveDocument(dto));
            }
            else
            {
                await ExecuteAsync($"{DapperHelper.UPDATE(TABLE, FIELDS, "")} WHERE Id = @Id", saveDocument(dto));
            }

            return true;
        }

        public async Task<bool> DocumentsExistsInGroup(int groupId)
        {
            return await ExecuteScalarAsync<bool>($"SELECT count(1) FROM {Tables.Name(Table.Documents)} WHERE TypeId = @groupId", new { groupId });
        }
    }
}
