﻿using Dapper;
using DFM.Utils;
using Appology.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Appology.MiCalendar.Model;
using Appology.MiCalendar.DTOs;
using Appology.Enums;

namespace Appology.MiCalendar.Repository
{
    public interface IEventRepository
    {
        Task<Event> GetAsync(Guid eventId);
        Task<IEnumerable<Event>> GetAllAsync(RequestEventDTO request);
        Task<IEnumerable<Event>> GetCurrentActivityAsync();
        Task<bool> EventExists(Guid eventId);
        Task<bool> EventUExists(string eventUid, string calendarUid);
        Task<(bool Status, Event e)> InsertOrUpdateAsync(Event e, bool multiInsert = false);
        Task<bool> DeleteAsync(Guid eventId);
        Task DeleteExtAsync(string eventUid, string calendarUid);
        Task<bool> EventsByTagExist(Guid tagID);
        Task<bool> EventExistsInCalendar(int calendarId);
        Task<bool> EventExistsAtStartTime(DateTime startDate, int calendarId);
        Task<Event> GetUEvent(string eventUId, string calendarUid);
        Task<string> GetLastStoredAlarm(Guid tagId);
    }

    public class EventRepository : IEventRepository
    {
        private readonly Func<IDbConnection> dbConnectionFactory;
        private static readonly string TABLE = Tables.Name(Table.Events);
        private static readonly string[] FIELDS = typeof(Event).DapperFields();

        public EventRepository(Func<IDbConnection> dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }

        public async Task<Event> GetAsync(Guid eventId)
        {
            using (var sql = dbConnectionFactory())
            {
                string sqlTxt = $@"
                    SELECT e.EventID, e.CalendarId, e.Reminder, u.Name, u.Avatar, e.Description, t.Name AS Subject, e.StartDate, e.EndDate, e.IsFullDay, t.ThemeColor, e.UserId, e.TagID, e.Tentative, e.EventUid, e.CalendarUid, e.Alarm, e.Provider
                    FROM {TABLE} e
                    LEFT JOIN {Tables.Name(Table.Users)} u
                    ON e.UserID = u.UserID
                    LEFT JOIN {Tables.Name(Table.Tags)} t
                    ON e.TagID = t.Id
                    WHERE EventId = @eventId";

                return (await sql.QueryAsync<Event>(sqlTxt, new { eventId })).FirstOrDefault();
            }
        }

        public async Task<Event> GetUEvent(string eventUId, string calendarUid)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Event>($"SELECT Modified, Created, EventId FROM {TABLE} WHERE EventUid = @eventUId AND CalendarUid = @calendarUid", new { eventUId, calendarUid })).FirstOrDefault();
            }
        }

        public async Task<IEnumerable<Event>> GetAllAsync(RequestEventDTO request)
        {
            using (var sql = dbConnectionFactory())
            {
                string sqlTxt = $@"
                    SELECT e.EventID,e.CalendarId,e.UserID,u.Name,u.Avatar,e.TagID,e.Description,e.StartDate,e.EndDate,e.IsFullDay, e.Tentative, t.ThemeColor, t.Name AS Subject, e.EventUid, e.CalendarUid, ty.InviteeIds, e.Alarm, e.Provider, e.Created, e.Modified, e.Reminder
                    FROM {TABLE} e
                    LEFT JOIN {Tables.Name(Table.Users)} u
                    ON e.UserID = u.UserID
                    LEFT JOIN {Tables.Name(Table.Tags)} t
                    ON e.TagID = t.Id
                    LEFT JOIN {Tables.Name(Table.Types)} ty
                    ON t.TypeID = ty.Id
                    WHERE CalendarId IN ({string.Join(",", request.CalendarIds)})
                    {(request.DateFilter != null && request.DateFilter.Frequency.HasValue ? $" AND {DateUtils.FilterDateSql(request.DateFilter)}" : null)}
                    {(request.Month != null && request.Year != null ? $" AND MONTH(StartDate) IN ({string.Join(",", request.Month)}) AND  YEAR(StartDate) IN ({string.Join(",", request.Year)})" : null)}
                    ORDER BY StartDate DESC";

                return (await sql.QueryAsync<Event>(sqlTxt)).ToArray();
            }
        }

        public async Task<string> GetLastStoredAlarm(Guid tagId)
        {
            using (var sql = dbConnectionFactory())
            {
                string sqlTxt = $@"
                    SELECT Alarm FROM {TABLE}
                    WHERE TagId = @tagId AND Alarm IS NOT NULL
                    GROUP BY TagId, Alarm
                    ORDER BY MAX(Created) DESC";

                return (await sql.QueryAsync<string>(sqlTxt, new { tagId })).FirstOrDefault();
            }
        }

        public async Task<IEnumerable<Event>> GetCurrentActivityAsync()
        {
            using (var sql = dbConnectionFactory())
            {
                string sqlTxt = $@"
                    SELECT e.CalendarId, e.UserID, e.TagID, e.Description, e.StartDate, e.EndDate, t.Name AS Subject, ty.InviteeIds
                    FROM {TABLE} e
                    LEFT JOIN {Tables.Name(Table.Tags)} t
                    ON e.TagID = t.Id
                    LEFT JOIN {Tables.Name(Table.Types)} ty
                    ON t.TypeID = ty.Id
                    WHERE CAST(StartDate as Date) = CAST(GetDate() as Date)
                    ORDER BY StartDate DESC";

                return (await sql.QueryAsync<Event>(sqlTxt)).ToArray();
            }
        }

        public async Task<bool> EventUExists(string eventUId, string calendarUid)
        {
            using (var sql = dbConnectionFactory())
            {
                string sqlTxt = $"SELECT count(1) FROM {TABLE} WHERE EventUid = {eventUId} AND CalendarUid = {calendarUid}";
                return await sql.ExecuteScalarAsync<bool>($"SELECT count(1) FROM {TABLE} WHERE EventUid = @eventUId AND CalendarUid = @calendarUid", new { eventUId, calendarUid });
            }
        }


        public async Task<bool> EventExistsAtStartTime(DateTime startDate, int calendarId)
        {
            using (var sql = dbConnectionFactory())
            {
                return await sql.ExecuteScalarAsync<bool>($"SELECT count(1) FROM {TABLE} WHERE StartDate = @StartDate AND CalendarId = @calendarId", new { StartDate = startDate, calendarId });
            }
        }

        public async Task<bool> EventExists(Guid eventId)
        {
            using (var sql = dbConnectionFactory())
            {
                return await sql.ExecuteScalarAsync<bool>($"SELECT count(1) FROM {TABLE} WHERE EventID = @EventId", new { EventID = eventId});
            }
        }

        public async Task<bool> EventsByTagExist(Guid tagID)
        {
            using (var sql = dbConnectionFactory())
            {
                return await sql.ExecuteScalarAsync<bool>($"SELECT count(1) FROM {TABLE} WHERE TagID = @tagId", new { @tagID });
            }
        }

        public async Task<(bool Status, Event e)> InsertOrUpdateAsync(Event e, bool multiInsert = false)
        {
            using (var sql = dbConnectionFactory())
            {
                try
                {
                    Guid eventId = eventId = e.EventID != Guid.Empty ? e.EventID : Guid.NewGuid();

                    Func<Event, object> saveEvent = (Event e) =>
                        new
                        {
                            eventId,
                            calendarId = e.CalendarId,
                            userId = e.UserID,
                            description = e.Description,
                            startDate = e.StartDate,
                            endDate = e.EndDate ?? null,
                            tentative = e.Tentative,
                            tagID = e.TagID ?? Guid.Empty,
                            isFullDay = e.IsFullDay,
                            eventUid = e.EventUid,
                            calendarUid = e.CalendarUid,
                            alarm = e.Alarm,
                            provider = e.Provider,
                            reminder = e.Reminder,
                            modified = DateUtils.FromTimeZoneToUtc(DateUtils.DateTime())
                        };

                    if (multiInsert)
                    {
                        await sql.ExecuteAsync($"{DapperHelper.INSERT(TABLE, FIELDS)}", saveEvent(e));
                    }
                    else
                    {
                        if (!await EventExists(e.EventID))
                        {
                            await sql.ExecuteAsync($"{DapperHelper.INSERT(TABLE, FIELDS)}", saveEvent(e));
                        }
                        else
                        {
                            await sql.ExecuteAsync($"{DapperHelper.UPDATE(TABLE, FIELDS, "")} WHERE EventID = @eventId", saveEvent(e));
                        }
                    }

                    return (true, await GetAsync(eventId));
                }
                catch (Exception exp)
                {
                    string.IsNullOrEmpty(exp.Message);
                    return (false, null);
                }
            }
        }

        public async Task DeleteExtAsync(string eventUid, string calendarUid)
        {
            using var sql = dbConnectionFactory();
            await sql.ExecuteAsync($@"{DapperHelper.DELETE(TABLE)} WHERE EventUid = @eventUid AND CalendarUid = @calendarUid", new { eventUid, calendarUid });
        }

        public async Task<bool> DeleteAsync(Guid eventId)
        {
            using (var sql = dbConnectionFactory())
            {
                try
                {
                    var delete = await sql.ExecuteAsync($@"{DapperHelper.DELETE(TABLE)} WHERE EventID = @EventID", new { EventID = eventId });
                    return true;
                }
                catch(Exception exp)
                {
                    string.IsNullOrEmpty(exp.Message);
                    return false;
                }
            }
        }

        public async Task<bool> EventExistsInCalendar(int calendarId)
        {
            using (var sql = dbConnectionFactory())
            {
                return await sql.ExecuteScalarAsync<bool>($"SELECT count(1) FROM {TABLE} WHERE CalendarId = @CalendarId", new { CalendarId = calendarId });
            }
        }
    }
}