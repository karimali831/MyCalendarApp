using Dapper;
using DFM.Utils;
using Appology.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Appology.MiCalendar.Model;
using Appology.Enums;

namespace Appology.MiCalendar.Repository
{
    public interface IDocumentRepository
    {
        Task<IEnumerable<Document>> GetAllByUserIdAsync(Guid userId);
        Task<IEnumerable<Document>> GetAllByTypeIdAsync(int typeId);
        Task<Document> GetAsync(Guid Id);
        Task<bool> InsertOrUpdateAsync(Document dto);
        Task<bool> MoveAsync(Guid docId, int moveToId);
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


        public async Task<IEnumerable<Document>> GetAllByUserIdAsync(Guid userId)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Document>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE UserCreatedId = @userId", new { userId })).ToArray();
            }
        }

        public async Task<IEnumerable<Document>> GetAllByTypeIdAsync(int typeId)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Document>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE TypeId = @typeId", new { typeId })).ToArray();
            }
        }

        public async Task<Document> GetAsync(Guid Id)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Document>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE Id = @Id", new { Id })).FirstOrDefault();
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
    }
}
