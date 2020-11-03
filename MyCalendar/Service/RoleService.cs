using MyCalendar.Model;
using MyCalendar.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyCalendar.Service
{
    public interface IRoleService
    {
        Task<IEnumerable<Role>> GetAllAsync();
        Task<Role> GetAsync(Guid Id);
    }

    public class RoleService : IRoleService
    {
        private readonly IRoleRepository roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            this.roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        }

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            return await roleRepository.GetAllAsync();
        }

        public async Task<Role> GetAsync(Guid Id)
        {
            return await roleRepository.GetAsync(Id);
        }
    }
}
