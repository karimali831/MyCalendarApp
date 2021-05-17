using Dapper;
using DFM.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Appology.MiCalendar.Model;
using Appology.Enums;
using Appology.Repository;

namespace Appology.MiCalendar.Repository
{
    public interface ITagRepository
    {
        Task<Tag> GetAsync(Guid tagID);
        Task<bool> UpdateUserTagsAsync(IList<Tag> tags, Guid userID);
        Task<bool> UserTagExists(Guid Id);
        Task DeleteTagByIdAsync(Guid tagID);
        Task DeleteAllUserTagsAsync(Guid userID);
    }

    public class TagRepository : DapperBaseRepository, ITagRepository
    {
        private static readonly string TABLE = Tables.Name(Table.Tags);
        private static readonly string[] FIELDS = typeof(Tag).DapperFields();

        public TagRepository(Func<IDbConnection> dbConnectionFactory) : base(dbConnectionFactory) { }

        public async Task<Tag> GetAsync(Guid tagID)
        {
            return await QueryFirstOrDefaultAsync<Tag>(@$" 
                SELECT t.Id, t.UserId, t.TypeId, t.Name, t.ThemeColor, t.TargetFrequency, t.TargetValue, t.TargetUnit, ty.InviteeIds, ty.Name AS TypeName
                FROM {TABLE} t
                LEFT JOIN {Tables.Name(Table.Types)} ty
                ON t.TypeID = ty.Id
                WHERE t.Id = @tagID", new { tagID });
        }

        public async Task<bool> UserTagExists(Guid Id)
        {
            return await ExecuteScalarAsync<bool>($"SELECT count(1) FROM {TABLE} WHERE Id= @Id", new { Id });
        }

        public async Task DeleteTagByIdAsync(Guid tagID)
        {
            await ExecuteAsync($"DELETE FROM {TABLE} WHERE Id = @TagID", new { TagID = tagID });
        }

        public async Task DeleteAllUserTagsAsync(Guid userID)
        {
            await ExecuteAsync($"DELETE FROM {TABLE} WHERE UserID = @UserID", new { UserID = userID });
        }

        public async Task<bool> UpdateUserTagsAsync(IList<Tag> tags, Guid userID)
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
                            themeColor = t.ThemeColor,
                            targetValue = t.TargetUnit != "disable" ? t.TargetValue : null,
                            targetUnit = t.TargetUnit,
                            targetFrequency = t.TargetUnit != "disable" ? t.TargetFrequency : null,
                            startDayOfWeek = t.StartDayOfWeek
                        };

                    var existing = await UserTagExists(tag.Id);

                    if (existing == false)
                    {
                        tag.Id = Guid.NewGuid();
                        await ExecuteAsync($"{DapperHelper.INSERT(TABLE, FIELDS)}", saveTag(tag));
                    }
                    else
                    {
                        await ExecuteAsync($"{DapperHelper.UPDATE(TABLE, FIELDS, "")} WHERE Id = @id", saveTag(tag));
                    }
                }
            }

            return true;
        }
    }
}
