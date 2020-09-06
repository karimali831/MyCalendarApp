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
    public interface IEventRepository
    {
        Task<IEnumerable<Event>> GetAllAsync();
        Task<bool> EventExists(Guid eventId);
        Task<bool> InsertOrUpdateAsync(Event e);
        Task<bool> DeleteAsync(Guid eventId);
    }

    public class EventRepository : IEventRepository
    {
        private readonly Func<IDbConnection> dbConnectionFactory;
        private static readonly string TABLE = "Events";
        private static readonly string[] FIELDS = typeof(Event).DapperFields();

        public EventRepository(Func<IDbConnection> dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }

        public async Task<IEnumerable<Event>> GetAllAsync()
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Event>($"{DapperHelper.SELECT(TABLE, FIELDS)}")).ToArray();
            }
        }

  

        public async Task<bool> EventExists(Guid eventId)
        {
            using (var sql = dbConnectionFactory())
            {
                return await sql.ExecuteScalarAsync<bool>($"SELECT count(1) FROM {TABLE} WHERE EventID = @EventId", new { EventID = eventId});
            }
        }

        public async Task<bool> InsertOrUpdateAsync(Event e)
        {
            using (var sql = dbConnectionFactory())
            {
                try
                {
                    var existing = await EventExists(e.EventID);

                    Func<Event, object> saveEvent = (Event e) =>
                        new
                        {
                            eventId = e.EventID,
                            subject = e.Subject,
                            description = e.Description,
                            startDate = e.StartDate,
                            endDate = e.EndDate,
                            themeColor = e.ThemeColor,
                            isFullDay = e.IsFullDay
                        };

                    if (existing == false)
                    {
                        e.EventID = Guid.NewGuid();
                        await sql.ExecuteAsync($"{DapperHelper.INSERT(TABLE, FIELDS)}", saveEvent(e));
                    }
                    else
                    {
                        await sql.ExecuteAsync($"{DapperHelper.UPDATE(TABLE, FIELDS, "")} WHERE EventID = @eventId", saveEvent(e));
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
