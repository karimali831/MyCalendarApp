using Appology.Model;
using Appology.Repository;
using Appology.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Appology.Service
{
    public interface IFeatureRoleService
    {
        Task<IEnumerable<FeatureRole>> GetAllAsync();
        Task<IEnumerable<Group>> GetGroupsAsync();
        Task<FeatureRole> GetAsync(Guid Id);
        Task<IEnumerable<Group>> AccessibleGroups(IEnumerable<Guid> roleIds);
        Task<IEnumerable<Feature>> AccessibleFeatures(IEnumerable<Guid> roleIds);
    }

    public class FeatureRoleService : IFeatureRoleService
    {
        public static readonly string cachePrefix = typeof(FeatureRoleService).FullName;
        private readonly IFeatureRoleRepository featureRoleRepository;
        private readonly IFeatureRepository featureRepository;
        private readonly IGroupRepository groupRepository;
        private readonly IRoleRepository roleRepository;
        private readonly ICacheService cache;

        public FeatureRoleService(
            IFeatureRoleRepository featureRoleRepository, 
            IGroupRepository groupRepository,
            IFeatureRepository featureRepository,
            IRoleRepository roleRepository,
            ICacheService cache)
        {
            this.featureRoleRepository = featureRoleRepository ?? throw new ArgumentNullException(nameof(featureRoleRepository));
            this.featureRepository = featureRepository ?? throw new ArgumentNullException(nameof(featureRepository));
            this.groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
            this.roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<IEnumerable<FeatureRole>> GetAllAsync()
        {
            return await cache.GetAsync(
                $"{cachePrefix}.{nameof(GetAllAsync)}",
                 async () => await featureRoleRepository.GetAllAsync()
            );
        }

        public async Task<IEnumerable<Group>> GetGroupsAsync()
        {
            return await cache.GetAsync(
                $"{cachePrefix}.{nameof(GetGroupsAsync)}",
                 async () => await groupRepository.GetAllAsync()
            );
        }

        public async Task<FeatureRole> GetAsync(Guid Id)
        {
            return await cache.GetAsync(
                $"{cachePrefix}.{nameof(GetAsync)}",
                async () => await featureRoleRepository.GetAsync(Id)
            );
        }

        public async Task<IEnumerable<Group>> AccessibleGroups(IEnumerable<Guid> roleIds)
        {
            return await cache.GetAsync(
                $"{cachePrefix}.{nameof(AccessibleGroups)}",
                async () =>
                {
                    var accessibleGroups = new List<Group>();
                    var featureGroupRoles = await featureRoleRepository.GetFeatureGroupRolesAsync();

                    foreach (var featureGroupRole in featureGroupRoles)
                    {
                        var superAdminRole = (await roleRepository.GetAllAsync())
                            .Where(x => x.Superadmin == true)
                            .Select(x => x.Id);

                        var group = await groupRepository.GetAsync(featureGroupRole.GroupId);

                        if (group != null)
                        {
                            if (roleIds.Any(x => superAdminRole.Any(r => r == x)))
                            {
                                accessibleGroups.Add(group);
                            }
                            else if (featureGroupRole.RoleIdsList.Any(x => roleIds.Any(r => r == x)))
                            {
                                accessibleGroups.Add(group);
                            }
                        }
                    }

                    return accessibleGroups;
                }
            );
        }

        public async Task<IEnumerable<Feature>> AccessibleFeatures(IEnumerable<Guid> roleIds)
        {
            return await cache.GetAsync(
                $"{cachePrefix}.{nameof(AccessibleFeatures)}",
                async () =>
                {
                    var accessibleFeatures = new List<Feature>();
                    var featureGroupRoles = await featureRoleRepository.GetFeatureGroupRolesAsync();

                    foreach (var featureGroupRole in featureGroupRoles)
                    {
                        var superAdminRole = (await roleRepository.GetAllAsync())
                            .Where(x => x.Superadmin == true)
                            .Select(x => x.Id);

                        var feature = await featureRepository.GetAsync(featureGroupRole.FeatureId);

                        if (feature != null)
                        {
                            if (roleIds.Any(x => superAdminRole.Any(r => r == x)))
                            {
                                accessibleFeatures.Add(feature);
                            }
                            else if (featureGroupRole.RoleIdsList.Any(x => roleIds.Any(r => r == x)))
                            {
                                accessibleFeatures.Add(feature);
                            }
                        }
                    }

                    return accessibleFeatures;
                }
            );
        }
    }
}
