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

namespace Appology.Write.Repository
{
    public interface IDocumentRepository
    {
        Task<IList<DocumentTitles>> GetDocTitlesByFolderId(int typeId, Guid userId);
        Task<IEnumerable<Document>> GetAllByTypeIdAsync(int typeId);
        Task<Document> GetAsync(Guid Id, Guid userId);
        Task<bool> InsertOrUpdateAsync(Document dto);
        Task<bool> MoveAsync(Guid docId, int moveToId);
        Task<bool> DocumentsExistsInGroup(int groupId);
        Task<bool> DeleteDocument(Guid docId);
        Task<IList<DocumentTitles>> GetDocTitlesByDocIds(IEnumerable<Guid> docIds);
        Task<IList<DocumentTitles>> SearchDocuments(string filter, Guid userId);
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


        public async Task<IList<DocumentTitles>> GetDocTitlesByFolderId(int typeId, Guid userId)
        {
            using (var sql = dbConnectionFactory())
            {
                string sqlTxt = $@"
                    SELECT d.Id, d.Title, d.EditedDate, EditedById
                    FROM {TABLE} AS d
                    LEFT JOIN {Tables.Name(Table.Types)} AS t
                    ON d.TypeId = t.Id
                    WHERE d.TypeId = {typeId}
                    AND (t.UserCreatedId = '{userId}'
                    OR (',' + RTRIM(t.InviteeIds) + ',') LIKE '%,{userId},%')
                    ORDER BY EditedDate DESC";

                return (await sql.QueryAsync<DocumentTitles>(sqlTxt)).ToArray();
            }
        }

        public async Task<IList<DocumentTitles>> GetDocTitlesByDocIds(IEnumerable<Guid> docIds)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<DocumentTitles>($"SELECT Id, Title, EditedDate, EditedById FROM {TABLE} WHERE Id IN @docIds", new { docIds })).ToList();
            }
        }

        public async Task<IList<DocumentTitles>> SearchDocuments(string filter, Guid userId)
        {
            using (var sql = dbConnectionFactory())
            {
                string sqlTxt = $@"
                    SELECT d.Id, d.Title
                    FROM {TABLE} AS d
                    LEFT JOIN {Tables.Name(Table.Types)} AS t
                    ON d.TypeId = t.Id
                    WHERE Title LIKE '%{filter}%'
                    AND (t.UserCreatedId = '{userId}'
                    OR (',' + RTRIM(t.InviteeIds) + ',') LIKE '%,{userId},%')
                    ORDER BY EditedDate DESC";

                return (await sql.QueryAsync<DocumentTitles>(sqlTxt)).ToList();
            }
        }


        public async Task<IEnumerable<Document>> GetAllByTypeIdAsync(int typeId)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Document>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE TypeId = @typeId", new { typeId })).ToArray();
            }
        }

        public async Task<bool> DeleteDocument(Guid docId)
        {
            using (var sql = dbConnectionFactory())
            {
                try
                {
                    await sql.ExecuteAsync($"{DapperHelper.DELETE(TABLE)} WHERE Id = @docId", new { docId });
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
                    SELECT d.Id, d.TypeId, d.Title, d.Text, d.CreatedDate, d.EditedDate, d.UserCreatedId, d.EditedById
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

        public async Task<bool> InsertOrUpdateAsync(Document dto)
        {
            using (var sql = dbConnectionFactory())
            {
                try
                {
                    Func<Document, object> saveDocument = (Document d) =>
                        new
                        {
                            id = d.Id,
                            typeId = d.TypeId,
                            title = d.Title,
                            text = d.Text,
                            userCreatedId = d.UserCreatedId,
                            editedDate = DateUtils.FromTimeZoneToUtc(DateUtils.DateTime()),
                            editedById = d.EditedById,
                            createdDate = d.CreatedDate
                        };

                    var existing = await DocumentExists(dto.Id);


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
