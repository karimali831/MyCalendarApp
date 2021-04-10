using Appology.DTOs;
using Appology.Enums;
using Appology.MiCalendar.Helpers;
using Appology.Model;
using Appology.Repository;
using Appology.Write.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Appology.Service
{
    public interface ITypeService
    {
        Task<IEnumerable<Types>> GetAllByUserIdAsync(Guid userId, TypeGroup? groupId = null, bool userCreatedOnly = true);
        Task<IEnumerable<Types>> GetUserTypesAsync(Guid userId, TypeGroup groupId);
        Task<Types> GetAsync(int Id);
        Task<bool> UpdateTypeAsync(Types type);
        Task<(bool Status, Types Calendar)> AddTypeAsync(TypeDTO type);
        Task<(bool Status, string Msg)> DeleteTypeAsync(int Id, Guid userId);
        Task<bool> MoveTypeAsync(int Id, int? moveToId = null);
        Task<int[]> GetAllIdsByParentTypeIdAsync(int superTypeId);
        Task<bool> UpdateInvitees(string invitees, Guid userId);
        Task<IEnumerable<Types>> GetAllUserTypesAsync(Guid userId, TypeGroup? groupId = null, bool userCreatedOnly = true);
    }

    public class TypeService : ITypeService
    {
        public static readonly string cachePrefix = typeof(TypeService).FullName;
        private readonly ITypeRepository typeRepository;
        private readonly IUserRepository userRepo;
        private readonly ICacheService cache;

        public TypeService(ITypeRepository typeRepository, IUserRepository userRepo, ICacheService cache)
        {
            this.typeRepository = typeRepository ?? throw new ArgumentNullException(nameof(TypeRepository));
            this.userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<IEnumerable<Types>> GetAllByUserIdAsync(Guid userId, TypeGroup? groupId, bool userCreatedOnly = true)
        {
            string cacheTypeName = groupId.HasValue ? groupId.Value.ToString() : "all";

            return await cache.GetAsync(
                $"{cachePrefix}.{nameof(GetAllByUserIdAsync)}.{cacheTypeName}.userCreatedOnly-{userCreatedOnly}",
                async () =>
                {
                    var result = new List<Types>();
                    var userTypes = (await GetAllUserTypesAsync(userId, groupId, userCreatedOnly))
                        .Where(x => x.SuperTypeId == null);

                    foreach (var userType in userTypes)
                    {
                        userType.Children = await UserTagsTree(userId, userType, groupId, userCreatedOnly);
                        userType.Collaborators = await GetCollaborators(userId, userType.UserCreatedId, userType.InviteeIdsList.ToList());

                        result.Add(userType);
                    }

                    return result;
                }
            );
        }

        private async Task<IList<Collaborator>> GetCollaborators(Guid userId, Guid creatorId, IList<Guid> inviteeIds)
        {
            inviteeIds.Add(creatorId);

            return (await userRepo.GetCollaboratorsAsync(inviteeIds))
                .Select(x =>
                {
                    x.Title = x.CollaboratorId == creatorId ? "Creator" : "Invitee";
                    x.Avatar = CalendarUtils.AvatarSrc(x.CollaboratorId, x.Avatar, x.Name);
                    x.ShowOnTree = x.CollaboratorId != userId;
                    return x;
                })
                .ToList();
        }

        private async Task<IEnumerable<Types>> GetAllByGroupAsync(TypeGroup groupId)
        {
            return await cache.GetAsync(
                $"{cachePrefix}.{nameof(GetAllByGroupAsync)}.{groupId}",
                async () => await typeRepository.GetAllByGroupAsync(groupId)
            );
        }

        private async Task<IEnumerable<Types>> UserTagsTree(Guid userId, Types element, TypeGroup? groupId, bool userCreatedOnly = true)
        {
            var childUserTypes = new List<Types>();
            var children = (await GetAllUserTypesAsync(userId, groupId, userCreatedOnly))
                .Where(x => x.SuperTypeId == element.Id);

            element.Children = children;

            foreach (var child in element.Children)
            {
                await UserTagsTree(userId, child, groupId);

                child.Collaborators = await GetCollaborators(userId, child.UserCreatedId, child.InviteeIdsList.ToList());
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

        public async Task<IEnumerable<Types>> GetAllUserTypesAsync(Guid userId, TypeGroup? groupId, bool userCreatedOnly = true)
        {
            string cacheTypeName = groupId.HasValue ? groupId.Value.ToString() : "all";

            return await cache.GetAsync(
                $"{cachePrefix}.{nameof(GetAllUserTypesAsync)}.{cacheTypeName}.userCreatedOnly-{userCreatedOnly}",
                async () => await typeRepository.GetAllByUserIdAsync(userId, groupId, userCreatedOnly)
            );
        }

        public async Task<Types> GetAsync(int Id)
        {
            return await typeRepository.GetAsync(Id);
        }

        public async Task<bool> UpdateTypeAsync(Types type)
        {
            cache.RemoveAll(cachePrefix);
            return await typeRepository.UpdateTypeAsync(type);
        }

        public async Task<(bool Status, Types Calendar)> AddTypeAsync(TypeDTO type)
        {
            cache.RemoveAll(cachePrefix);
            return await typeRepository.AddTypeAsync(type);
        }

        public async Task<(bool Status, string Msg)> DeleteTypeAsync(int Id, Guid userId)
        {
            cache.RemoveAll(cachePrefix);
            var status = await typeRepository.DeleteTypeAsync(Id);
            return (status, status ? "Deleted successfully" : "An error occured");
        }

        public async Task<bool> MoveTypeAsync(int Id, int? moveToId = null)
        {
            cache.RemoveAll(cachePrefix);
            return await typeRepository.MoveTypeAsync(Id, moveToId);
        }

        public async Task<int[]> GetAllIdsByParentTypeIdAsync(int superTypeId)
        {
            return await cache.GetAsync(
                $"{cachePrefix}.{nameof(GetAllIdsByParentTypeIdAsync)}",
                async () => await typeRepository.GetAllIdsByParentTypeIdAsync(superTypeId)
            );
        }

        public async Task<bool> UpdateInvitees(string invitees, Guid userId)
        {
            cache.RemoveAll(cachePrefix);
            return await typeRepository.UpdateInvitees(invitees, userId);
        }
    }
}
