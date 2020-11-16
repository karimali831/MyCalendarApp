using MyCalendar.Model;
using MyCalendar.Repository;
using MyCalendar.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCalendar.Service
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
        private readonly IFeatureRoleRepository featureRoleRepository;
        private readonly IFeatureRepository featureRepository;
        private readonly IGroupRepository groupRepository;
        private readonly IRoleRepository roleRepository;

        public FeatureRoleService(
            IFeatureRoleRepository featureRoleRepository, 
            IGroupRepository groupRepository,
            IFeatureRepository featureRepository,
            IRoleRepository roleRepository)
        {
            this.featureRoleRepository = featureRoleRepository ?? throw new ArgumentNullException(nameof(featureRoleRepository));
            this.featureRepository = featureRepository ?? throw new ArgumentNullException(nameof(featureRepository));
            this.groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
            this.roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        }

        public async Task<IEnumerable<FeatureRole>> GetAllAsync()
        {
            return await featureRoleRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Group>> GetGroupsAsync()
        {
            return await groupRepository.GetAllAsync();
        }

        public async Task<FeatureRole> GetAsync(Guid Id)
        {
            return await featureRoleRepository.GetAsync(Id);
        }

        public async Task<IEnumerable<Group>> AccessibleGroups(IEnumerable<Guid> roleIds)
        {
            var accessibleGroups = new List<Group>();
            var featureGroupRoles = await featureRoleRepository.GetFeatureGroupRolesAsync();

            foreach (var featureGroupRole in featureGroupRoles)
            {
                var superAdminRole = (await roleRepository.GetAllAsync())
                    .Where(x => x.Superadmin == true)
                    .Select(x => x.Id);

                var group = await groupRepository.GetAsync(featureGroupRole.GroupId);

                if (roleIds.Any(x => superAdminRole.Any(r => r == x)))
                {
                    accessibleGroups.Add(group);
                }
                else if (featureGroupRole.RoleIdsList.Any(x => roleIds.Any(r => r == x)))
                {
                    accessibleGroups.Add(group);
                }
            }

            return accessibleGroups;
        }

        public async Task<IEnumerable<Feature>> AccessibleFeatures(IEnumerable<Guid> roleIds)
        {
            var accessibleFeatures = new List<Feature>();
            var featureGroupRoles = await featureRoleRepository.GetFeatureGroupRolesAsync();

            foreach (var featureGroupRole in featureGroupRoles)
            {
                var superAdminRole = (await roleRepository.GetAllAsync())
                    .Where(x => x.Superadmin == true)
                    .Select(x => x.Id);

                var feature = await featureRepository.GetAsync(featureGroupRole.FeatureId);

                if (roleIds.Any(x => superAdminRole.Any(r => r == x)))
                {
                    accessibleFeatures.Add(feature);
                }
                else if (featureGroupRole.RoleIdsList.Any(x => roleIds.Any(r => r == x)))
                {
                    accessibleFeatures.Add(feature);
                }
            }

            return accessibleFeatures;
        }
    }
}
