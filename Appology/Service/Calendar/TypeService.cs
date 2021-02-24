using Appology.DTOs;
using Appology.Enums;
using Appology.MiCalendar.DTOs;
using Appology.MiCalendar.Model;
using Appology.MiCalendar.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Appology.MiCalendar.Service
{
    public interface ITypeService
    {
        Task<IEnumerable<Types>> GetAllByGroupAsync(TypeGroup groupId);
        Task<IEnumerable<Types>> GetAllByUserIdAsync(Guid userId, TypeGroup? groupId = null);
        Task<IEnumerable<Types>> UserTagsTree(Guid userId, Types element, TypeGroup? groupId);
        Task<IEnumerable<Types>> GetUserTypesAsync(Guid userId, TypeGroup groupId);
        Task<Types> GetAsync(int Id);
        Task<bool> UpdateTypeAsync(Types type);
        Task<(bool Status, Types Calendar)> AddTypeAsync(TypeDTO type);
        Task<(bool Status, string Msg)> DeleteTypeAsync(int Id, Guid userId);
        Task<bool> MoveTypeAsync(int Id, int? moveToId = null);
        Task<int[]> GetAllIdsByParentTypeIdAsync(int superTypeId);
        Task<bool> UpdateInvitees(string invitees, Guid userId);
        Task<IEnumerable<Types>> GetAllUserTypesAsync(Guid userId);
    }

    public class TypeService : ITypeService
    {
        private readonly ITypeRepository typeRepository;

        public TypeService(ITypeRepository typeRepository)
        {
            this.typeRepository = typeRepository ?? throw new ArgumentNullException(nameof(TypeRepository));
        }

        public async Task<IEnumerable<Types>> GetAllByUserIdAsync(Guid userId, TypeGroup? groupId)
        {
            var result = new List<Types>();
            var userTypes = (await typeRepository.GetAllByUserIdAsync(userId, groupId))
                .Where(x => x.SuperTypeId == null);
            
            foreach (var userType in userTypes)
            {
                userType.Children = await UserTagsTree(userId, userType, groupId);
                result.Add(userType);
            }

            return result;
        }

        public async Task<IEnumerable<Types>> GetAllByGroupAsync(TypeGroup groupId)
        {
            return await typeRepository.GetAllByGroupAsync(groupId);
        }

        public async Task<IEnumerable<Types>> UserTagsTree(Guid userId, Types element, TypeGroup? groupId)
        {
            var childUserTypes = new List<Types>();
            var children = (await typeRepository.GetAllByUserIdAsync(userId, groupId))
                .Where(x => x.SuperTypeId == element.Id);

            element.Children = children;

            foreach (var child in element.Children)
            {
                await UserTagsTree(userId, child, groupId);
                childUserTypes.Add(child);
            }

            return childUserTypes;
        }

        public async Task<IEnumerable<Types>> GetUserTypesAsync(Guid userId, TypeGroup groupId)
        {
            return (await GetAllByGroupAsync(groupId))
                .Where(x => x.UserCreatedId == userId || (x.InviteeIdsList != null && x.InviteeIdsList.Contains(userId)))
                .OrderByDescending(x => x.UserCreatedId == userId);
        }

        public async Task<IEnumerable<Types>> GetAllUserTypesAsync(Guid userId)
        {
            return await typeRepository.GetAllByUserIdAsync(userId);
        }

        public async Task<Types> GetAsync(int Id)
        {
            return await typeRepository.GetAsync(Id);
        }

        public async Task<bool> UpdateTypeAsync(Types type)
        {
            return await typeRepository.UpdateTypeAsync(type);
        }

        public async Task<(bool Status, Types Calendar)> AddTypeAsync(TypeDTO type)
        {
            return await typeRepository.AddTypeAsync(type);
        }

        public async Task<(bool Status, string Msg)> DeleteTypeAsync(int Id, Guid userId)
        {
            var status = await typeRepository.DeleteTypeAsync(Id);
            return (status, status ? "Deleted successfully" : "An error occured");
        }

        public async Task<bool> MoveTypeAsync(int Id, int? moveToId = null)
        {
            return await typeRepository.MoveTypeAsync(Id, moveToId);
        }

        public async Task<int[]> GetAllIdsByParentTypeIdAsync(int superTypeId)
        {
            return await typeRepository.GetAllIdsByParentTypeIdAsync(superTypeId);
        }

        public async Task<bool> UpdateInvitees(string invitees, Guid userId)
        {
            return await typeRepository.UpdateInvitees(invitees, userId);
        }
    }
}
