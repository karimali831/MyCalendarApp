using Appology.Enums;
using Appology.MiFinance.DTOs;
using Appology.MiFinance.Model;
using Appology.Repository;
using DFM.Utils;
using System;
using System.Collections.Generic;
using System.Data;
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

    public class RemindersRepository : DapperBaseRepository, IRemindersRepository
    {
        private static readonly string TABLE = Tables.Name(Table.Reminders);
        private static readonly string[] DTOFIELDS = typeof(ReminderDTO).DapperFields();

        public RemindersRepository(Func<IDbConnection> dbConnectionFactory) : base(dbConnectionFactory) { }

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

            return await QueryAsync<Reminder>(sqlTxt);
        }

        public async Task<bool> ReminderExists(string notes)
        {
            return await ExecuteScalarAsync<bool>($"SELECT 1 WHERE EXISTS (SELECT 1 FROM {TABLE} WHERE PATINDEX(@Notes, [Notes]) <> 0)", new { Notes = notes });
        }

        public async Task InsertAsync(ReminderDTO dto)
        {
            await ExecuteAsync($"{DapperHelper.INSERT(TABLE, DTOFIELDS)}", dto);
        }

        public async Task HideAsync(int Id)
        {
            await ExecuteAsync($"UPDATE {TABLE} SET Display = 0 WHERE Id = @Id", new { Id });
        }
    }
}
