﻿using MyCalendar.DTOs;
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
        Task<bool> UpdateAsync(User user);
        Task<User> GetByUserIDAsync(Guid userID);
    }
    ;
    public class UserService : IUserService
    {
        private readonly IUserRepository userRepository;
        private readonly ITagService tagService;

        public UserService(IUserRepository userRepository, ITagService tagService)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.tagService = tagService ?? throw new ArgumentNullException(nameof(tagService));
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await userRepository.GetAllAsync();
        }

        public async Task<User> GetAsync(int passcode)
        {
            return await userRepository.GetAsync(passcode); 
        }

        public async Task<bool> UpdateAsync(User user)
        {
            return await userRepository.UpdateAsync(user);
        }

        public async Task<User> GetByUserIDAsync(Guid userID)
        {
            return await userRepository.GetByUserIDAsync(userID);
        }
    }
}
