using MyCalendar.Model;
using MyCalendar.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyCalendar.Service
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User> GetAsync(int passcode);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository userRepository;

        public UserService(IUserRepository userRepository)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await userRepository.GetAllAsync();
        }

        public async Task<User> GetAsync(int passcode)
        {
            return await userRepository.GetAsync(passcode);
        }

    }
}
