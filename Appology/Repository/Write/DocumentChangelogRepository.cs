using Dapper;
using DFM.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Appology.Enums;
using Appology.Write.Model;
using Appology.Repository;

namespace Appology.Write.Repository
{
    public interface IDocumentChangelogRepository
    {
        Task<IEnumerable<DocumentChangelog>> GetDocChangelogTitles(Guid docId);
        Task<(string OldText, string NewText)> GetDocChangelogTexts(int Id, Guid userId);
        Task<bool> InsertChangelog(DocumentChangelog docChangeLog);

    }

    public class DocumentChangelogRepository : DapperBaseRepository, IDocumentChangelogRepository
    {
        private static readonly string TABLE = Tables.Name(Table.DocumentChangelog);
        private static readonly string[] FIELDS = typeof(DocumentChangelog).DapperFields();

        public DocumentChangelogRepository(Func<IDbConnection> dbConnectionFactory) : base(dbConnectionFactory) { }


        public async Task<IEnumerable<DocumentChangelog>> GetDocChangelogTitles(Guid docId)
        {
            return await QueryAsync<DocumentChangelog>($"SELECT Id, DocId, UserId, Date FROM {TABLE} WHERE DocId = @docId", new { docId });
        }

        public async Task<(string OldText, string NewText)> GetDocChangelogTexts(int Id, Guid userId)
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

            return await QueryFirstOrDefaultAsync<(string, string)>(sqlTxt);
        }

        public async Task<bool> InsertChangelog(DocumentChangelog docChangeLog)
        {
            return await ExecuteAsync($"{DapperHelper.INSERT(TABLE, FIELDS)}", docChangeLog);
        }
    }
}
