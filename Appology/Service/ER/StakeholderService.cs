using Appology.Enums;
using Appology.ER.Enums;
using Appology.ER.Model;
using Appology.ER.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Appology.ER.Service
{
    public interface IStakeholderService
    {
        Task<(Stakeholder Stakeholder, bool Status)> GetAsync(Guid Id);
        Task<IEnumerable<Stakeholder>> GetAllAsync(Stakeholders stakeholderId, string filter = null);
        Task<(Stakeholder stakeholder, string Message)> RegisterAsync(Stakeholder stakeholder);

        Task<bool> UpdatePaymentIds(IList<string> paymentIds, Guid userId);
    }

    public class StakeholderService : IStakeholderService
    {
        private readonly IStakeholderRepository stakeholderRepository;

        public StakeholderService(IStakeholderRepository stakeholderRepository)
        {
            this.stakeholderRepository = stakeholderRepository ?? throw new ArgumentNullException(nameof(stakeholderRepository));
        }

        public async Task<IEnumerable<Stakeholder>> GetAllAsync(Stakeholders stakeholderId, string filter = null)
        {
            return await stakeholderRepository.GetAllAsync(stakeholderId, filter);
        }

        public async Task<(Stakeholder Stakeholder, bool Status)> GetAsync(Guid Id)
        {
            return await stakeholderRepository.GetAsync(Id);
        }

        public async Task<(Stakeholder stakeholder, string Message)> RegisterAsync(Stakeholder stakeholder)
        {
            string message;
            Stakeholder newStakeholder = null;

            if (await stakeholderRepository.UserDetailsExists(nameof(Stakeholder.Email), stakeholder.Email, stakeholder.StakeholderId))
            {
                message = "Email already exists";
            }
            else if (
                await stakeholderRepository.UserDetailsExists(nameof(Stakeholder.Address1), stakeholder.Address1, stakeholder.StakeholderId) &&
                await stakeholderRepository.UserDetailsExists(nameof(Stakeholder.FirstName), stakeholder.FirstName, stakeholder.StakeholderId))
            {
                message = "Matching name and address exists";
            }
            else if (await stakeholderRepository.UserDetailsExists(nameof(Stakeholder.ContactNo1), stakeholder.ContactNo1, stakeholder.StakeholderId))
            {
                message = "Matching primary contact number exists";
            }
            else
            {
                var register = await stakeholderRepository.RegisterAsync(stakeholder);

                if (register.Status == true)
                {
                    newStakeholder = register.newStakeholder;
                    message = "Registration successful";
                }
                else
                {
                    message = "An error occured";
                }
            }

            return (newStakeholder, message);
        }

        public async Task<bool> UpdatePaymentIds(IList<string> paymentIds, Guid userId)
        {
            return await stakeholderRepository.UpdatePaymentIds(paymentIds, userId);
        }
    }
}
