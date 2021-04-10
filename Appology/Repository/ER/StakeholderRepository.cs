using DFM.Utils;
using Appology.Enums;
using Appology.ER.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Appology.ER.Enums;
using Appology.Repository;

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

    public class StakeholderRepository : DapperBaseRepository, IStakeholderRepository
    {
        private static readonly string TABLE = Tables.Name(Table.Stakeholders);
        private static readonly string[] FIELDS = typeof(Stakeholder).DapperFields();

        public StakeholderRepository(Func<IDbConnection> dbConnectionFactory) : base(dbConnectionFactory) { }

        public async Task<(Stakeholder Stakeholder, bool Status)> GetAsync(Guid Id)
        {
            var stakeholder =  await QueryFirstOrDefaultAsync<Stakeholder>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE Id = @Id", new { Id });
            return (stakeholder, stakeholder != null);
        }

        public async Task<IEnumerable<Stakeholder>> GetAllAsync(Stakeholders stakeholderId, string filter = null)
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

            return await QueryAsync<Stakeholder>(sqlTxt);
            
        }

        public async Task<bool> UserDetailsExists(string field, string value, Stakeholders stakeholderId)
        {
            return await ExecuteScalarAsync<bool>($"SELECT count(1) FROM {TABLE} WHERE {field} = @Value AND StakeholderId = @StakeholderId",
                new { 
                    Value = value.Trim(),
                    StakeholderId = (int)stakeholderId
                }
            );
        }

        public async Task<(Stakeholder newStakeholder, bool Status)> RegisterAsync(Stakeholder stakeholder)
        {

            stakeholder.Id = Guid.NewGuid();
            await ExecuteAsync($"{DapperHelper.INSERT(TABLE, FIELDS)}", stakeholder);

            var newStakeholder = await GetAsync(stakeholder.Id);
            return (newStakeholder.Status ? newStakeholder.Stakeholder : null, newStakeholder.Status);
        }

        public async Task<bool> UpdatePaymentIds(IList<string> paymentIds, Guid userId)
        {

            return await ExecuteAsync($"UPDATE {TABLE} SET PaymentIds = @PaymentIds WHERE UserID = @UserId",
                new
                {
                    PaymentIds = paymentIds != null && paymentIds.Any() ? string.Join(",", paymentIds) : null,
                    UserId = userId
                });
        }

    }
}
