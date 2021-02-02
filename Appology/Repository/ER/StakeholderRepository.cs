using Dapper;
using DFM.Utils;
using Appology.Enums;
using Appology.ER.Model;
using Appology.Helpers;
using Appology.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Appology.ER.Enums;

namespace Appology.ER.Repository
{
    public interface IStakeholderRepository
    {
        Task<(Stakeholder Stakeholder, bool Status)> GetAsync(Guid Id);
        Task<IEnumerable<Stakeholder>> GetAllAsync(Stakeholders stakeholderId, string filter = null);
        Task<(Stakeholder newStakeholder, bool Status)> RegisterAsync(Stakeholder stakeholder);
        Task<bool> UserDetailsExists(string field, string value, Stakeholders stakeholderId);
        Task<bool> UpdatePaymentIds(IList<string> paymentIds, Guid userId);
    }

    public class StakeholderRepository : IStakeholderRepository
    {
        private readonly Func<IDbConnection> dbConnectionFactory;
        private static readonly string TABLE = Tables.Name(Table.Stakeholders);
        private static readonly string[] FIELDS = typeof(Stakeholder).DapperFields();

        public StakeholderRepository(Func<IDbConnection> dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }

        public async Task<(Stakeholder Stakeholder, bool Status)> GetAsync(Guid Id)
        {
            using (var sql = dbConnectionFactory())
            {
                try
                {
                    var stakeholder =  (await sql.QueryAsync<Stakeholder>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE Id = @Id", new { Id })).FirstOrDefault();
                    return (stakeholder, true);
                }
                catch (Exception exp)
                {
                    string.IsNullOrEmpty(exp.Message);
                    return (null, false);
                }
            }
        }

        public async Task<IEnumerable<Stakeholder>> GetAllAsync(Stakeholders stakeholderId, string filter = null)
        {
            using (var sql = dbConnectionFactory())
            {
                string sqlTxt = $@"
                    {DapperHelper.SELECT(TABLE, FIELDS)}
                    WHERE StakeholderId = {(int)stakeholderId}
                    {(filter != null ? $@" AND (
                        FirstName LIKE '%{filter}%' OR
                        LastName LIKE '%{filter}%' OR
                        Email LIKE '%{filter}%' OR
                        Address1 LIKE '%{filter}%' OR
                        Postcode LIKE '%{filter}%' OR
                        Id LIKE '%{filter}%')" : "")}";

                return (await sql.QueryAsync<Stakeholder>(sqlTxt)).ToArray();
            }
        }

        public async Task<bool> UserDetailsExists(string field, string value, Stakeholders stakeholderId)
        {
            using (var sql = dbConnectionFactory())
            {
                return await sql.ExecuteScalarAsync<bool>($"SELECT count(1) FROM {TABLE} WHERE {field} = @Value AND StakeholderId = @StakeholderId",
                    new { 
                        Value = value.Trim(),
                        StakeholderId = (int)stakeholderId
                    }
                );
            }
        }

        public async Task<(Stakeholder newStakeholder, bool Status)> RegisterAsync(Stakeholder stakeholder)
        {
            using (var sql = dbConnectionFactory())
            {
                try
                {
                    stakeholder.Id = Guid.NewGuid();
                    await sql.ExecuteAsync($"{DapperHelper.INSERT(TABLE, FIELDS)}", stakeholder);

                    var newStakeholder = await GetAsync(stakeholder.Id);
                    return (newStakeholder.Status ? newStakeholder.Stakeholder : null, newStakeholder.Status);
                }
                catch (Exception exp)
                {
                    string.IsNullOrEmpty(exp.Message);
                    return (null, false);
                }
            }
        }

        public async Task<bool> UpdatePaymentIds(IList<string> paymentIds, Guid userId)
        {
            using (var sql = dbConnectionFactory())
            {
                try
                {
                    await sql.ExecuteAsync($"UPDATE {TABLE} SET PaymentIds = @PaymentIds WHERE UserID = @UserId",
                        new
                        {
                            PaymentIds = paymentIds != null && paymentIds.Any() ? string.Join(",", paymentIds) : null,
                            UserId = userId
                        });


                    return true;
                }
                catch (Exception exp)
                {
                    string.IsNullOrEmpty(exp.Message);
                    return false;
                }
            }
        }
    }
}
