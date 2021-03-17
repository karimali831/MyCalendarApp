using Dapper;
using DFM.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Appology.Enums;
using Appology.Write.Model;

namespace Appology.Write.Repository
{
    public interface IDocumentChangelogRepository
    {
        Task<IEnumerable<DocumentChangelog>> GetDocChangelogTitles(Guid docId);
        Task<(string OldText, string NewText)> GetDocChangelogTexts(int Id, Guid userId);
        Task<bool> InsertChangelog(DocumentChangelog docChangeLog);

    }

    public class DocumentChangelogRepository : IDocumentChangelogRepository
    {
        private readonly Func<IDbConnection> dbConnectionFactory;
        private static readonly string TABLE = Tables.Name(Table.DocumentChangelog);
        private static readonly string[] FIELDS = typeof(DocumentChangelog).DapperFields();

        public DocumentChangelogRepository(Func<IDbConnection> dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }


        public async Task<IEnumerable<DocumentChangelog>> GetDocChangelogTitles(Guid docId)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<DocumentChangelog>($"SELECT Id, DocId, UserId, Date FROM {TABLE} WHERE DocId = @docId", new { docId })).ToArray();
            }
        }

        public async Task<(string OldText, string NewText)> GetDocChangelogTexts(int Id, Guid userId)
        {
            using (var sql = dbConnectionFactory())
            {
                string sqlTxt = $@"
                    SELECT dc.OldText, dc.NewText
                    FROM {TABLE} AS dc
                    LEFT JOIN {Tables.Name(Table.Documents)} AS d
                    ON d.Id = dc.DocId
                    LEFT JOIN {Tables.Name(Table.Types)} AS t
                    ON d.TypeId = t.Id
                    WHERE dc.Id = '{Id}'
                    AND (t.UserCreatedId = '{userId}'
                    OR (',' + RTRIM(t.InviteeIds) + ',') LIKE '%,{userId},%')";

                return (await sql.QueryAsync<(string, string)>(sqlTxt)).FirstOrDefault();
            }
        }

        public async Task<bool> InsertChangelog(DocumentChangelog docChangeLog)
        {
            using (var sql = dbConnectionFactory())
            {
                try
                {
                    await sql.ExecuteAsync($"{DapperHelper.INSERT(TABLE, FIELDS)}", docChangeLog);
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
