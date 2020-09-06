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
    }

    public class EventRepository : IEventRepository
    {
        private readonly Func<IDbConnection> dbConnectionFactory;
        //private static readonly string TABLE = "Events";
        private static readonly string[] FIELDS = typeof(Event).DapperFields();

        public EventRepository(Func<IDbConnection> dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }

        public async Task<IEnumerable<Event>> GetAllAsync()
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Event>($"SELECT * FROM Events")).ToArray();
                //return (await sql.QueryAsync<Event>($"{DapperHelper.SELECT(TABLE, FIELDS)}")).ToArray();
            }
        }
    }
}
