using Appology.Enums;
using Appology.MiFinance.DTOs;
using Appology.MiFinance.Model;
using Dapper;
using DFM.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Appology.MiFinance.Repository
{
    public interface IRemindersRepository
    {
        Task<IEnumerable<Reminder>> GetAllAsync();
        Task InsertAsync(ReminderDTO dto);
        Task HideAsync(int Id);
        Task<bool> ReminderExists(string notes);
    }

    public class RemindersRepository : IRemindersRepository
    {
        private readonly Func<IDbConnection> dbConnectionFactory;
        private static readonly string TABLE = Tables.Name(Table.Reminders);
        private static readonly string[] FIELDS = typeof(Reminder).DapperFields();
        private static readonly string[] DTOFIELDS = typeof(ReminderDTO).DapperFields();

        public RemindersRepository(Func<IDbConnection> dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }

        public async Task<IEnumerable<Reminder>> GetAllAsync()
        {
            string sqlTxt = $@"
                SELECT 
                    r.Id,
                    r.Notes,
                    r.DueDate,
                    r.AddedDate,
                    r.Display,
                    r.Priority as _priority,
                    c.Name AS Category
                FROM 
	                {TABLE} as r
	            LEFT JOIN {Tables.Name(Table.FinanceCategories)} c
                    ON c.Id = r.CatId";

            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Reminder>(sqlTxt)).ToArray();
            }
        }

        public async Task<bool> ReminderExists(string notes)
        {
            using (var sql = dbConnectionFactory())
            {
                var exists = await sql.QueryAsync<bool>($"SELECT 1 WHERE EXISTS (SELECT 1 FROM {TABLE} WHERE PATINDEX(@Notes, [Notes]) <> 0)", new { Notes = notes });
                return exists.Any();
            }
        }

        public async Task InsertAsync(ReminderDTO dto)
        {
            using (var sql = dbConnectionFactory())
            {
                await sql.ExecuteAsync($"{DapperHelper.INSERT(TABLE, DTOFIELDS)}", dto);
            }
        }

        public async Task HideAsync(int Id)
        {
            using (var sql = dbConnectionFactory())
            {
                await sql.ExecuteAsync($"UPDATE {TABLE} SET Display = 0 WHERE Id = @Id", new { Id });
            }
        }
    }
}
