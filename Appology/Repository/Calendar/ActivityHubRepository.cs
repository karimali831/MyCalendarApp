using DFM.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Appology.MiCalendar.Model;
using Appology.Enums;
using Appology.Repository;
using Appology.Helpers;
using Appology.DTOs;

namespace Appology.MiCalendar.Repository
{
    public interface IActivityHubRepository
    {
        Task<bool> AddAsync(ActivityHub activity);
        Task<IEnumerable<ActivityHub>> GetAllByUserIdAsync(Guid userId, BaseDateFilter dateFilter);
    }

    public class ActivityHubRepository : DapperBaseRepository, IActivityHubRepository
    {
        private static readonly string TABLE = Tables.Name(Table.ActivityHub);
        private static readonly string[] FIELDS = typeof(ActivityHub).DapperFields();

        public ActivityHubRepository(Func<IDbConnection> dbConnectionFactory) : base(dbConnectionFactory) { }


        public async Task<bool> AddAsync(ActivityHub activity)
        {
            return await ExecuteAsync($"{DapperHelper.INSERT(TABLE, FIELDS)}", activity);
        }
        public async Task<IEnumerable<ActivityHub>> GetAllByUserIdAsync(Guid userId, BaseDateFilter dateFilter)
        {
            string sqlTxt = $@"
                SELECT a.Id, a.UserId, a.TagId, a.Minutes, t.ThemeColor, t.WeeklyHourlyTarget, t.Name AS Subject, t.TypeId AS TagGroupId, ty.Name AS TagGroupName, ty.InviteeIds, u.Name, u.Avatar
                FROM {TABLE} as a
                LEFT JOIN {Tables.Name(Table.Users)} u
                ON a.UserId = u.UserID
                LEFT JOIN {Tables.Name(Table.Tags)} t
                ON a.TagID = t.Id
                LEFT JOIN {Tables.Name(Table.Types)} ty
                ON t.TypeID = ty.Id
                WHERE a.UserId = @userId
                AND {DateUtils.FilterDateSql(dateFilter)}
            ";

            return await QueryAsync<ActivityHub>(sqlTxt, new { userId });
        }
    }
}
