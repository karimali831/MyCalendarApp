using Appology.Enums;
using Appology.MiFinance.Model;
using Appology.Repository;
using DFM.Utils;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Appology.MiFinance.Repository
{
    public interface ISettingRepository
    {
        Task<Setting> GetAsync();
        Task UpdateAsync(Setting settings);
        Task UpdateCashBalanceAsync(decimal cashBalance);
    }

    public class SettingRepository : DapperBaseRepository, ISettingRepository
    {
        private static readonly string TABLE = Tables.Name(Table.Settings);
        private static readonly string[] FIELDS = typeof(Setting).DapperFields();

        public SettingRepository(Func<IDbConnection> dbConnectionFactory) : base(dbConnectionFactory) { }


        public async Task<Setting> GetAsync()
        {
            return await QueryFirstOrDefaultAsync<Setting>($"{DapperHelper.SELECT(TABLE, FIELDS)}");
        }

        public async Task UpdateAsync(Setting settings)
        {
            string sqlTxt = $@"
                UPDATE {TABLE} SET
                AvailableCredit = @AvailableCredit,
                AvailableCash = @AvailableCash,
                StartingDate = @StartingDate
            ";

            await ExecuteAsync(sqlTxt, new 
            {
                settings.AvailableCredit,
                settings.AvailableCash,
                settings.StartingDate
            });
        }

        public async Task UpdateCashBalanceAsync(decimal cashBalance)
        {
            await ExecuteAsync($"UPDATE {TABLE} SET AvailableCash += @CashBalance", new { CashBalance = cashBalance });
        }
    }
}
