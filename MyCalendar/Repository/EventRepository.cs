﻿using Dapper;
using DFM.Utils;
using MyCalendar.DTOs;
using MyCalendar.Helpers;
using MyCalendar.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCalendar.Repository
{
    public interface IEventRepository
    {
        Task<Event> GetAsync(Guid eventId);
        Task<IEnumerable<Event>> GetAllAsync();
        Task<IEnumerable<Event>> GetCurrentActivityAsync();
        Task<bool> EventExists(Guid eventId);
        Task<bool> InsertOrUpdateAsync(Model.EventDTO dto);
        Task<bool> MultiInsertAsync(IEnumerable<Model.EventDTO> dto);
        Task<bool> DeleteAsync(Guid eventId);
        Task<bool> EventsByTagExist(Guid tagID);
    }

    public class EventRepository : IEventRepository
    {
        private readonly Func<IDbConnection> dbConnectionFactory;
        private static readonly string TABLE = "Events";
        private static readonly string[] FIELDS = typeof(Event).DapperFields();
        private static readonly string[] DTOFIELDS = typeof(Model.EventDTO).DapperFields();

        public EventRepository(Func<IDbConnection> dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }

        public async Task<Event> GetAsync(Guid eventId)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Event>($"{DapperHelper.SELECT(TABLE, DTOFIELDS)} WHERE EventID = @eventId", new { eventId })).FirstOrDefault();
            }
        }

        public async Task<IEnumerable<Event>> GetAllAsync()
        {
            using (var sql = dbConnectionFactory())
            {
                string sqlTxt = $@"
                    SELECT e.EventID,e.UserID,e.TagID,e.Description,e.StartDate,e.EndDate,e.IsFullDay, e.Tentative, t.ThemeColor, t.Name AS Subject, t.Privacy
                    FROM Events e
                    LEFT JOIN Tags t
                    ON e.TagID = t.Id";

                return (await sql.QueryAsync<Event>(sqlTxt)).ToArray();
            }
        }

        public async Task<IEnumerable<Event>> GetCurrentActivityAsync()
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Event>($"{DapperHelper.SELECT(TABLE, DTOFIELDS)} WHERE GETDATE() >= StartDate AND GETDATE() < EndDate")).ToArray();
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

        public async Task<bool> InsertOrUpdateAsync(Model.EventDTO dto)
        {
            using (var sql = dbConnectionFactory())
            {
                try
                {
                    Func<Model.EventDTO, object> saveEvent = (Model.EventDTO e) =>
                        new
                        {
                            eventId = e.EventID,
                            userId = e.UserID,
                            description = e.Description,
                            startDate = e.StartDate,
                            endDate = e.EndDate ?? null,
                            tentative = e.Tentative,
                            tagID = e.TagID,
                            isFullDay = e.IsFullDay
                        };

                    var existing = await EventExists(dto.EventID);

                    if (existing == false)
                    {
                        dto.EventID = Guid.NewGuid();
                        await sql.ExecuteAsync($"{DapperHelper.INSERT(TABLE, DTOFIELDS)}", saveEvent(dto));
                    }
                    else
                    {
                        await sql.ExecuteAsync($"{DapperHelper.UPDATE(TABLE, DTOFIELDS, "")} WHERE EventID = @eventId", saveEvent(dto));
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

        public async Task<bool> MultiInsertAsync(IEnumerable<Model.EventDTO> dto)
        {
            using (var sql = dbConnectionFactory())
            {
                try
                {
                    foreach (var e in dto)
                    {
                        Func<Model.EventDTO, object> saveEvent = (Model.EventDTO e) =>
                            new
                            {
                                eventId = Guid.NewGuid(),
                                userId = e.UserID,
                                description = e.Description,
                                startDate = Utils.FromTimeZoneToUtc(e.StartDate),
                                endDate = e.EndDate.HasValue ? Utils.FromTimeZoneToUtc(e.EndDate.Value) : (DateTime?)null,
                                tentative = e.Tentative,
                                tagID = e.TagID,
                                isFullDay = e.IsFullDay
                            };

                        await sql.ExecuteAsync($"{DapperHelper.INSERT(TABLE, DTOFIELDS)}", saveEvent(e));

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
    }
}
