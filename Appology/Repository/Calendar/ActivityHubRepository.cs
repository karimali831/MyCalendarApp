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
using Appology.MiCalendar.DTOs;

namespace Appology.MiCalendar.Repository
{
    public interface IActivityHubRepository
    {
        Task<ActivityHub> GetAsync(Guid Id);
        Task<bool> DeleteAsync(Guid Id);
        Task<bool> AddAsync(ActivityHub activity);
        Task<IEnumerable<ActivityHubStatsMonth>> GetStats(Guid userId, BaseDateFilter dateFilter);
        Task<IEnumerable<ActivityHub>> GetAllByUserIdAsync(Guid userId, BaseDateFilter dateFilter);
    }

    public class ActivityHubRepository : DapperBaseRepository, IActivityHubRepository
    {
        private static readonly string TABLE = Tables.Name(Table.ActivityHub);
        private static readonly string[] FIELDS = typeof(ActivityHub).DapperFields();

        public ActivityHubRepository(Func<IDbConnection> dbConnectionFactory) : base(dbConnectionFactory) { }

        public async Task<ActivityHub> GetAsync(Guid Id)
        {
            return await QueryFirstOrDefaultAsync<ActivityHub>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE Id = @Id", new { Id } );
        }

        public async Task<bool> DeleteAsync(Guid Id)
        {
            return await ExecuteAsync($"{DapperHelper.DELETE(TABLE)} WHERE Id = @Id", new { Id });
        }

        public async Task<bool> AddAsync(ActivityHub activity)
        {
            return await ExecuteAsync($"{DapperHelper.INSERT(TABLE, FIELDS)}", activity);
        }

        public async Task<IEnumerable<ActivityHub>> GetAllByUserIdAsync(Guid userId, BaseDateFilter dateFilter)
        {
            string sqlTxt = $@"
                SELECT a.Id, a.UserId, a.TagId, a.Value, t.ThemeColor, t.TargetFrequency, t.TargetValue, t.TargetUnit, a.Date, t.Name AS Subject, t.TypeId AS TagGroupId, ty.Name AS TagGroupName, ty.InviteeIds, u.Name, u.Avatar
                FROM {TABLE} as a
                LEFT JOIN {Tables.Name(Table.Users)} u
                ON a.UserId = u.UserID
                LEFT JOIN {Tables.Name(Table.Tags)} t
                ON a.TagID = t.Id
                LEFT JOIN {Tables.Name(Table.Types)} ty
                ON t.TypeID = ty.Id
                WHERE a.UserId = @userId
                AND {DateUtils.FilterDateSql(dateFilter)}
                ORDER BY a.Date DESC, t.Name ASC
            ";

            return await QueryAsync<ActivityHub>(sqlTxt, new { userId });
        }

        public async Task<IEnumerable<ActivityHubStatsMonth>> GetStats(Guid userId, BaseDateFilter dateFilter)
        {

            //--AND MONTH(StartDate) = DATEPART(MONTH, DATEADD(MONTH, @prevMonthInt, GETDATE()))
            //--AND YEAR(StartDate) = YEAR(GETDATE())

            var eventDateFilter = new DateFilter
            {
                DateField = "StartDate",
                Frequency = dateFilter.Frequency,
                FromDateRange = dateFilter.FromDateRange,
                ToDateRange = dateFilter.ToDateRange
            };

            var hubDateFilter = new DateFilter
            {
                DateField = "Date",
                Frequency = dateFilter.Frequency,
                FromDateRange = dateFilter.FromDateRange,
                ToDateRange = dateFilter.ToDateRange
            };

            string sqlTxt = $@"
                SELECT TagId, Name AS TagName, TargetUnit, StartDayOfWeek, EndDayOfWeek, SUM(TotalValue) AS TotalValue
                FROM
                (
                    SELECT e.TagID, t.Name, t.TargetUnit, t.StartDayOfWeek, t.EndDayOfWeek, SUM(DATEDIFF(MINUTE, e.StartDate, e.EndDate)) AS TotalValue
                    FROM {Tables.Name(Table.Events)}  AS e
	                LEFT JOIN {Tables.Name(Table.Tags)}  AS t
	                ON e.TagID = t.Id
	                WHERE e.Reminder = 0 AND e.UserID = @userId
                    AND {DateUtils.FilterDateSql(eventDateFilter)}
	                GROUP BY e.TagID, t.Name, t.TargetUnit, t.StartDayOfWeek, t.EndDayOfWeek

                    UNION ALL

                    SELECT h.TagId, t.Name, t.TargetUnit, t.StartDayOfWeek, t.EndDayOfWeek, SUM(h.Value)
                    FROM {TABLE} as h
	                LEFT JOIN {Tables.Name(Table.Tags)} AS t
	                ON h.TagId = t.Id
                    WHERE h.UserId = @userId
                    AND {DateUtils.FilterDateSql(hubDateFilter)}
	                GROUP BY h.TagId, t.Name, t.TargetUnit, t.StartDayOfWeek, t.EndDayOfWeek
                ) t
                WHERE TargetUnit != 'disable'
                GROUP BY TagID, Name, TargetUnit, StartDayOfWeek, EndDayOfWeek
            ";

            return await QueryAsync<ActivityHubStatsMonth>(sqlTxt, new { userId });
        }
    }
}
