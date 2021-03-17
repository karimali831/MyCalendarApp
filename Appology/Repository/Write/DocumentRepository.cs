using Dapper;
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

namespace Appology.Write.Repository
{
    public interface IDocumentRepository
    {
        //Task<IList<DocumentTitlesVM>> GetDocTitlesByFolderId(int typeId, Guid userId);
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
    }

    public class DocumentRepository : IDocumentRepository
    {
        private readonly Func<IDbConnection> dbConnectionFactory;
        private static readonly string TABLE = Tables.Name(Table.Documents);
        private static readonly string[] FIELDS = typeof(Document).DapperFields();

        public DocumentRepository(Func<IDbConnection> dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }


        //public async Task<IList<DocumentTitlesVM>> GetDocTitlesByFolderId(int typeId, Guid userId)
        //{
        //    using (var sql = dbConnectionFactory())
        //    {
        //        string sqlTxt = $@"
        //            SELECT d.Id, d.Title, d.EditedDate, d.EditedById, d.EditedAuto
        //            FROM {TABLE} AS d
        //            LEFT JOIN {Tables.Name(Table.Types)} AS t
        //            ON d.TypeId = t.Id
        //            WHERE d.TypeId = {typeId}
        //            AND (t.UserCreatedId = '{userId}'
        //            OR (',' + RTRIM(t.InviteeIds) + ',') LIKE '%,{userId},%')
        //            ORDER BY EditedDate DESC";

        //        return (await sql.QueryAsync<DocumentTitlesVM>(sqlTxt)).ToArray();
        //    }
        //}

        //public async Task<IList<DocumentTitlesVM>> GetDocTitlesByDocIds(IEnumerable<Guid> docIds)
        //{
        //    using (var sql = dbConnectionFactory())
        //    {
        //        return (await sql.QueryAsync<DocumentTitlesVM>($"SELECT Id, Title, EditedDate, EditedById, EditedAuto FROM {TABLE} WHERE Id IN @docIds", new { docIds })).ToList();
        //    }
        //}

        public async Task<IList<DocumentTitlesVM>> GetDocumentTitles(Guid userId)
        {
            using (var sql = dbConnectionFactory())
            {
                string sqlTxt = $@"
                    SELECT d.Id, d.TypeId, d.Title, d.EditedDate, d.EditedById, d.EditedAuto, d.Tags, d.UserCreatedId
                    FROM {TABLE} AS d
                    LEFT JOIN {Tables.Name(Table.Types)} AS t
                    ON d.TypeId = t.Id
                    WHERE  t.UserCreatedId = '{userId}'
                    OR (',' + RTRIM(t.InviteeIds) + ',') LIKE '%,{userId},%'
                    ORDER BY EditedDate DESC";

                return (await sql.QueryAsync<DocumentTitlesVM>(sqlTxt)).ToList();
            }
        }

        public async Task<DocumentTitlesVM> GetDocumentTitle(Guid docId)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<DocumentTitlesVM>($"SELECT Id, TypeId, Title, EditedDate, EditedById, EditedAuto, Tags, UserCreatedId FROM {TABLE} WHERE Id = @docId", new { docId })).FirstOrDefault();
            }
        }

        public async Task<IList<DocumentTitlesVM>> SearchDocumentsByFilter(IEnumerable<Guid> docIds, string filter, Guid userId)
        {
            using (var sql = dbConnectionFactory())
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

                return (await sql.QueryAsync<DocumentTitlesVM>(sqlTxt)).ToList();
            }
        }

        public async Task<IEnumerable<Document>> GetAllByTypeIdAsync(int typeId)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Document>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE TypeId = @typeId", new { typeId })).ToArray();
            }
        }

        public async Task<IEnumerable<(Guid DocId, string Tag)>> GetAllDocumentUserTags(Guid userId)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<(Guid, string)>($"SELECT Id, Tags FROM {TABLE} WHERE UserCreatedId = @userId AND datalength(tags)!=0", new { userId })).ToArray();
            }
        }

        public async Task<bool> DeleteDocument(Guid docId)
        {
            using (var sql = dbConnectionFactory())
            {
                try
                {
                    await sql.ExecuteAsync($"{DapperHelper.DELETE(TABLE)} WHERE Id = @docId", new { docId });
                    await sql.ExecuteAsync($"{DapperHelper.DELETE(Tables.Name(Table.DocumentChangelog))} WHERE DocId = @docId", new { docId });

                    return true;
                }
                catch (Exception exp)
                {
                    string.IsNullOrEmpty(exp.Message);
                    return false;
                }
            }
        }

        public async Task<Document> GetAsync(Guid Id, Guid userId)
        {
            using (var sql = dbConnectionFactory())
            {
                string sqlTxt = $@"
                    SELECT d.Id, d.TypeId, d.Title, d.Text, d.DraftText, d.CreatedDate, d.EditedDate, d.UserCreatedId, d.EditedById, d.EditedAuto, d.Tags
                    FROM {TABLE} AS d
                    LEFT JOIN {Tables.Name(Table.Types)} AS t
                    ON d.TypeId = t.Id
                    WHERE d.Id = '{Id}'
                    AND (t.UserCreatedId = '{userId}'
                    OR (',' + RTRIM(t.InviteeIds) + ',') LIKE '%,{userId},%')";

                return (await sql.QueryAsync<Document>(sqlTxt)).FirstOrDefault();
            }
        }

        public async Task<bool> DocumentExists(Guid docId)
        {
            using (var sql = dbConnectionFactory())
            {
                return await sql.ExecuteScalarAsync<bool>($"SELECT count(1) FROM {TABLE} WHERE Id = @Id", new { Id = docId });
            }
        }

        public async Task<bool> MoveAsync(Guid docId, int moveToId)
        {
            using (var sql = dbConnectionFactory())
            {
                try
                {
                    await sql.ExecuteAsync($"UPDATE {TABLE} SET TypeId = @moveToId WHERE Id = @docId", new { docId, moveToId });
                    return true;
                }
                catch (Exception exp)
                {
                    string.IsNullOrEmpty(exp.Message);
                    return false;
                }
            }
        }

        public async Task<bool> InsertOrUpdateAsync(DocumentDTO dto)
        {
            using (var sql = dbConnectionFactory())
            {
                try
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

                    var existing = await DocumentExists(dto.Id.Value);


                    if (existing == false)
                    {
                        await sql.ExecuteAsync($"{DapperHelper.INSERT(TABLE, FIELDS)}", saveDocument(dto));
                    }
                    else
                    {
                        await sql.ExecuteAsync($"{DapperHelper.UPDATE(TABLE, FIELDS, "")} WHERE Id = @Id", saveDocument(dto));
                    }

                    return true;
                }
                catch (Exception exp)
                {
                    string.IsNullOrEmpty(exp.Message);
                    return false;
                }
            }
        }

        public async Task<bool> DocumentsExistsInGroup(int groupId)
        {
            using (var sql = dbConnectionFactory())
            {
                return await sql.ExecuteScalarAsync<bool>($"SELECT count(1) FROM {Tables.Name(Table.Documents)} WHERE TypeId = @groupId", new { groupId });
            }
        }
    }
}
