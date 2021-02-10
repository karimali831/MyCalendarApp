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
        Task<IEnumerable<Types>> GetAllAsync();
        Task<IEnumerable<Types>> GetAllByUserIdAsync(Guid userId);
        Task<IEnumerable<Types>> UserTagsTree(Guid userId, Types element);
        Task<IEnumerable<Types>> GetUserTypesAsync(Guid userId);
        Task<Types> GetAsync(int Id);
        Task<bool> UpdateTypeAsync(Types type);
        Task<(bool Status, Types Calendar)> AddTypeAsync(TypeDTO type);
        Task<(bool Status, string Msg)> DeleteTypeAsync(int Id, Guid userId);
        Task<bool> MoveTypeAsync(int Id, int? moveToId = null);
    }

    public class TypeService : ITypeService
    {
        private readonly ITypeRepository typeRepository;

        public TypeService(ITypeRepository typeRepository)
        {
            this.typeRepository = typeRepository ?? throw new ArgumentNullException(nameof(TypeRepository));
        }

        public async Task<IEnumerable<Types>> GetAllByUserIdAsync(Guid userId)
        {
            var result = new List<Types>();
            var userTypes = (await typeRepository.GetAllByUserIdAsync(userId)).Where(x => x.SuperTypeId == null);
            
            foreach (var userType in userTypes)
            {
                userType.Children = await UserTagsTree(userId, userType);
                result.Add(userType);
            }

            return result;
        }

        public async Task<IEnumerable<Types>> GetAllAsync()
        {
            return (await typeRepository.GetAllAsync());
        }

        public async Task<IEnumerable<Types>> UserTagsTree(Guid userId, Types element)
        {
            var childUserTypes = new List<Types>();
            var children = (await typeRepository.GetAllByUserIdAsync(userId)).Where(x => x.SuperTypeId == element.Id);

            element.Children = children;

            foreach (var child in element.Children)
            {
                await UserTagsTree(userId, child);
                childUserTypes.Add(child);
            }

            return childUserTypes;
        }

        public async Task<IEnumerable<Types>> GetUserTypesAsync(Guid userId)
        {
            return (await GetAllAsync())
                .Where(x => x.UserCreatedId == userId || (x.InviteeIdsList != null && x.InviteeIdsList.Contains(userId)))
                .OrderByDescending(x => x.UserCreatedId == userId);
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
    }
}
