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
    public interface ICalendarRepository
    {
        Task<IEnumerable<Calendar>> GetAllAsync();
    }

    public class CalendarRepository : ICalendarRepository
    {
        private readonly Func<IDbConnection> dbConnectionFactory;
        private static readonly string TABLE = "Calendar";
        private static readonly string[] FIELDS = typeof(Calendar).DapperFields();

        public CalendarRepository(Func<IDbConnection> dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }

        public async Task<IEnumerable<Calendar>> GetAllAsync()
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Calendar>($"{DapperHelper.SELECT(TABLE, FIELDS)}")).ToArray();
            }
        }
    }
}
