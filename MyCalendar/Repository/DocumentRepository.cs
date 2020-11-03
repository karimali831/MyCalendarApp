using Dapper;
using DFM.Utils;
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
        Task<Document> GetAsync(Guid Id);
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

        public async Task<Document> GetAsync(Guid Id)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Document>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE Id = @Id", new { Id })).FirstOrDefault();
            }
        }

    }
}
