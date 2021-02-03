using Appology.Enums;
using Appology.MiFinance.Models;
using Dapper;
using DFM.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Appology.MiFinance.Repository
{
    public interface IMonzoRepository
    {
        Task<Monzo> MonzoAccountSummary();
        Task<IEnumerable<MonzoTransaction>> MonzoTransactions();
        Task InsertMonzoAccountSummary(Monzo accountSummary);
        Task InsertMonzoTransaction(MonzoTransaction transaction);
        Task<bool> TransactionExists(string transId);
        Task DeleteMonzoTransaction(string transId);
    }

    public class MonzoRepository : IMonzoRepository
    {
        private readonly Func<IDbConnection> dbConnectionFactory;
        private static readonly string TABLE = Tables.Name(Table.MonzoAccount);
        private static readonly string[] FIELDS = typeof(Monzo).DapperFields();
        private static readonly string[] TRANSFIELDS = typeof(MonzoTransaction).DapperFields();

        public MonzoRepository(Func<IDbConnection> dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }

        public async Task InsertMonzoAccountSummary(Monzo accountSummary)
        {
            using (var sql = dbConnectionFactory())
            {
                static object entry(Monzo t) =>
                    new
                    {
                        balance = t.Balance,
                        savingsBalance = t.SavingsBalance,
                        sortCode = t.SortCode,
                        accountNo = t.AccountNo,
                        spentToday = t.SpentToday,
                        jsonTransactions = JsonConvert.SerializeObject(t.Transactions),
                        created = DateTime.UtcNow
                    };

                await sql.QueryAsync<Monzo>($@"
                DELETE FROM {TABLE}
                INSERT INTO {TABLE} (Balance, SavingsBalance, SortCode, AccountNo, SpentToday, JsonTransactions, Created) VALUES (@balance, @savingsBalance, @sortCode, @accountNo, @spentToday, @jsonTransactions, @created)",
                    entry(accountSummary));
            }
        }

        public async Task InsertMonzoTransaction(MonzoTransaction transaction)
        {
            using (var sql = dbConnectionFactory())
            {
                await sql.ExecuteAsync($"{DapperHelper.INSERT(Tables.Name(Table.MonzoTransactions), TRANSFIELDS)}", transaction);
            }
        }

        public async Task<Monzo> MonzoAccountSummary()
        {
            using (var sql = dbConnectionFactory())
            {
                return
                    (await sql.QueryAsync<Monzo>($"SELECT TOP 1 * FROM {TABLE} ORDER BY created DESC"))
                        .Select(x => new Monzo
                        {
                            Balance = x.Balance,
                            SavingsBalance = x.SavingsBalance,
                            SortCode = x.SortCode,
                            AccountNo = x.AccountNo,
                            SpentToday = x.SpentToday,
                            Transactions = JsonConvert.DeserializeObject<IEnumerable<MonzoTransaction>>(x.JsonTransactions),
                            Created = x.Created
                        })
                        .FirstOrDefault();
            }
        }

        public async Task<IEnumerable<MonzoTransaction>> MonzoTransactions()
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<MonzoTransaction>($"{DapperHelper.SELECT(Tables.Name(Table.MonzoTransactions), TRANSFIELDS)}")).ToArray();
            }
        }

        public async Task<bool> TransactionExists(string transId)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.ExecuteScalarAsync<bool>($@"
                    SELECT count(1) FROM {Tables.Name(Table.MonzoTransactions)} WHERE Id = @Id",
                    new { Id = transId }
                ));
            }
        }

        public async Task DeleteMonzoTransaction(string transId)
        {
            using (var sql = dbConnectionFactory())
            {
                await sql.ExecuteAsync($"{DapperHelper.DELETE(Tables.Name(Table.MonzoTransactions))} WHERE Id = @Id", new { Id = transId });
            }
        }
    }
}
