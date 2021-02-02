using Appology.Enums;
using Appology.MiFinance.Model;
using Dapper;
using DFM.Utils;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Appology.MiFinance.Repository
{
    public interface ISettingRepository
    {
        Task<Setting> GetAsync();
        Task UpdateAsync(Setting settings);
        Task UpdateCashBalanceAsync(decimal cashBalance);
    }

    public class SettingRepository : ISettingRepository
    {
        private readonly Func<IDbConnection> dbConnectionFactory;
        private static readonly string TABLE = Tables.Name(Table.Settings);
        private static readonly string[] FIELDS = typeof(Setting).DapperFields();

        public SettingRepository(Func<IDbConnection> dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }


        public async Task<Setting> GetAsync()
        {
            using var sql = dbConnectionFactory();
            return (await sql.QueryAsync<Setting>($"{DapperHelper.SELECT(TABLE, FIELDS)}")).First();
        }

        public async Task UpdateAsync(Setting settings)
        {
            string sqlTxt = $@"
                UPDATE {TABLE} SET
                AvailableCredit = @AvailableCredit,
                AvailableCash = @AvailableCash,
                StartingDate = @StartingDate
            ";

            using var sql = dbConnectionFactory();

            await sql.ExecuteAsync(sqlTxt, new 
            {
                settings.AvailableCredit,
                settings.AvailableCash,
                settings.StartingDate
            });
        }

        public async Task UpdateCashBalanceAsync(decimal cashBalance)
        {
            using (var sql = dbConnectionFactory())
            {
                await sql.ExecuteAsync($"UPDATE {TABLE} SET AvailableCash += @CashBalance", new { CashBalance = cashBalance } );
            }
        }
    }
}
