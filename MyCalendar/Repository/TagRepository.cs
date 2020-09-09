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
    public interface ITagRepository
    {
        Task<IEnumerable<Tag>> GetTagsByUserAsync(Guid userID);
        Task<bool> UpdateUserTagsAsync(IEnumerable<Tag> tags);
        Task<bool> UserTagExists(Guid Id);
    }

    public class TagRepository : ITagRepository
    {
        private readonly Func<IDbConnection> dbConnectionFactory;
        private static readonly string TABLE = "Tags";
        private static readonly string[] FIELDS = typeof(Tag).DapperFields();

        public TagRepository(Func<IDbConnection> dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }


        public async Task<IEnumerable<Tag>> GetTagsByUserAsync(Guid userID)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Tag>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE UserID = @userID", new { userID })).ToArray();
            }
        }

        public async Task<bool> UserTagExists(Guid Id)
        {
            using (var sql = dbConnectionFactory())
            {
                return await sql.ExecuteScalarAsync<bool>($"SELECT count(1) FROM {TABLE} WHERE Id= @Id", new { Id });
            }
        }

        public async Task<bool> UpdateUserTagsAsync(IEnumerable<Tag> tags)
        {
            using (var sql = dbConnectionFactory())
            {
                try
                {
                    await sql.ExecuteAsync($"DELETE FROM {TABLE} WHERE UserID = @userID", new { userID = tags.First().UserID });

                    foreach (var tag in tags)
                    {
                        Func<Tag, object> saveTag = (Tag t) =>
                            new
                            {
                                id = t.Id,
                                userId = tags.First().UserID,
                                typeId = t.TypeID,
                                name = t.Name,
                                themeColor = t.ThemeColor
                            };

                        var existing = await UserTagExists(tag.Id);

                        if (existing == false)
                        {
                            tag.Id = Guid.NewGuid();
                            await sql.ExecuteAsync($"{DapperHelper.INSERT(TABLE, FIELDS)}", saveTag(tag));
                        }
                        else
                        {
                            await sql.ExecuteAsync($"{DapperHelper.UPDATE(TABLE, FIELDS, "")} WHERE Id = @id", saveTag(tag));
                        }
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
