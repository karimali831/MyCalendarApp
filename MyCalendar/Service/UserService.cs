using MyCalendar.Helpers;
using MyCalendar.Model;
using MyCalendar.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCalendar.Service
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User> GetAsync(int passcode);
        Task<bool> UpdateAsync(User user);
        Task<User> GetByUserIDAsync(Guid userID);
        Task<Tag> GetUserTagAysnc(Guid tagID);
        Task<List<string>> CurrentUserActivity(IEnumerable<Event> events);
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

        public async Task<Tag> GetUserTagAysnc(Guid tagID)
        {
            return await tagService.GetAsync(tagID);
        }

        public async Task<List<string>> CurrentUserActivity(IEnumerable<Event> events)
        {
            var currentActivity = new List<string>();

            if (events != null && events.Any())
            {
                foreach (var activity in events)
                {
                    string getName = (await GetByUserIDAsync(activity.UserID)).Name;
                    string label = (await GetUserTagAysnc(activity.TagID))?.Name ?? activity.Description;
                    string finishing = (activity.EndDate.HasValue ? "finishing " + Utils.FromUtcToTimeZone(activity.EndDate.Value).ToString("HH:mm") : "for the day");
                    string starting = Utils.FromUtcToTimeZone(activity.StartDate).ToString("HH:mm");

                    if (Utils.DateTime() >= Utils.FromUtcToTimeZone(activity.StartDate.AddHours(-4)) && Utils.DateTime() < Utils.FromUtcToTimeZone(activity.StartDate))
                    {
                        currentActivity.Add(string.Format("{0} has an upcoming event today - {1} starting {2}", getName, label, starting));
                    }
                    else
                    {
                        currentActivity.Add(string.Format("{0} currently at event - {1} {2}", getName, label, finishing));
                    }
                }
            }

            return currentActivity;
        }
    }
}
