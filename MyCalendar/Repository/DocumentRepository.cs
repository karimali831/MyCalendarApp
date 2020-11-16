using Dapper;
using DFM.Utils;
using MyCalendar.Helpers;
using MyCalendar.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace MyCalendar.Repository
{
    public interface IDocumentRepository
    {
        Task<IEnumerable<Document>> GetAllByUserIdAsync(Guid userId);
        Task<IEnumerable<Document>> GetAllByTypeIdAsync(int typeId);
        Task<Document> GetAsync(Guid Id);
        Task<bool> InsertOrUpdateAsync(Document dto);
    }

    public class DocumentRepository : IDocumentRepository
    {
        private readonly Func<IDbConnection> dbConnectionFactory;
        private static readonly string TABLE = "Documents";
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
                            editedDate = Utils.FromTimeZoneToUtc(Utils.DateTime()),
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
