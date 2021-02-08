﻿using Dapper;
using DFM.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Appology.MiCalendar.Model;
using Appology.Enums;

namespace Appology.MiCalendar.Repository
{
    public interface ITagRepository
    {
        Task<Tag> GetAsync(Guid tagID);
        Task<bool> UpdateUserTagsAsync(IEnumerable<Tag> tags, Guid userID);
        Task<bool> UserTagExists(Guid Id);
        Task DeleteTagByIdAsync(Guid tagID);
        Task DeleteAllUserTagsAsync(Guid userID);
    }

    public class TagRepository : ITagRepository
    {
        private readonly Func<IDbConnection> dbConnectionFactory;
        private static readonly string TABLE = Tables.Name(Table.Tags);
        private static readonly string[] FIELDS = typeof(Tag).DapperFields();

        public TagRepository(Func<IDbConnection> dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }

        public async Task<Tag> GetAsync(Guid tagID)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Tag>(@$" 
                    SELECT t.Id, t.UserId, t.TypeId, t.Name, t.ThemeColor, ty.InviteeIds, ty.Name AS TypeName
                    FROM {TABLE} t
                    LEFT JOIN {Tables.Name(Table.Types)} ty
                    ON t.TypeID = ty.Id
                    WHERE t.Id = @tagID", new { tagID })).FirstOrDefault();
            }
        }

        public async Task<bool> UserTagExists(Guid Id)
        {
            using (var sql = dbConnectionFactory())
            {
                return await sql.ExecuteScalarAsync<bool>($"SELECT count(1) FROM {TABLE} WHERE Id= @Id", new { Id });
            }
        }

        public async Task DeleteTagByIdAsync(Guid tagID)
        {
            using (var sql = dbConnectionFactory())
            {
                await sql.ExecuteAsync($"DELETE FROM {TABLE} WHERE Id = @TagID", new { TagID = tagID });
            }
        }

        public async Task DeleteAllUserTagsAsync(Guid userID)
        {
            using (var sql = dbConnectionFactory())
            {
                await sql.ExecuteAsync($"DELETE FROM {TABLE} WHERE UserID = @UserID", new { UserID = userID });
            }
        }

        public async Task<bool> UpdateUserTagsAsync(IEnumerable<Tag> tags, Guid userID)
        {
            using (var sql = dbConnectionFactory())
            {
                try
                {
                    foreach (var tag in tags)
                    {
                        // only update if tag is created by user
                        if (tag.UserID == Guid.Empty || tag.UserID == userID)
                        {
                            Func<Tag, object> saveTag = (Tag t) =>
                                new
                                {
                                    id = t.Id,
                                    userId = userID,
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