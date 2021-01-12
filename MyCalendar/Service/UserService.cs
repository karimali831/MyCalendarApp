using MyCalendar.Helpers;
using MyCalendar.Model;
using MyCalendar.Repository;
using MyCalendar.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Configuration;
using System.Web.UI;
using MyCalendar.Security;

namespace MyCalendar.Service
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User> GetUser(string email = null, string password = null);
        Task<bool> UpdateAsync(User user);
        Task<User> GetByUserIDAsync(Guid userID);
        Task<bool> UpdateUserTagsAsync(IEnumerable<Tag> tags, Guid userId);
        Task<Tag> GetUserTagAysnc(Guid tagID);
        Task<IList<User>> GetBuddys(Guid userId);
        Task<IEnumerable<Tag>> GetUserTags(Guid userId);
        Task<IEnumerable<Types>> UserCalendars(Guid userId, bool userCreated = false);
        Task<bool> RetainCalendarSelection(int[] calendarIds, Guid userId);
        Task<bool> RetainCalendarView(string view, Guid userId);
    }
    
    public class UserService : IUserService
    {
        private readonly IUserRepository userRepository;
        private readonly ITypeService typeService;
        private readonly ITagService tagService;
        private readonly ICronofyService cronofyService;
        private readonly ITagRepository tagRepository;

        public UserService(
            ITagService tagService, 
            IUserRepository userRepository, 
            ICronofyService cronofyService, 
            ITagRepository tagRepository,
            ITypeService typeService)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.tagService = tagService ?? throw new ArgumentNullException(nameof(tagService));
            this.cronofyService = cronofyService ?? throw new ArgumentNullException(nameof(cronofyService));
            this.tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
            this.typeService = typeService ?? throw new ArgumentNullException(nameof(typeService));
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await userRepository.GetAllAsync();
        }

        public async Task<IList<User>> GetBuddys(Guid userId)
        {
            var buddyIds = (await userRepository.GetByUserIDAsync(userId))?.BuddyIds ?? null;
            var buddyList = new List<User>();

            if (!string.IsNullOrEmpty(buddyIds))
            {
                var getBuddyList = buddyIds.Split(',').Select(x => Guid.Parse(x)).ToList();

                foreach (var buddy in getBuddyList)
                {
                    var buddyProfile = await userRepository.GetByUserIDAsync(buddy);
                    buddyList.Add(buddyProfile);
                }
            }

            return buddyList;
        }

        public async Task<IEnumerable<Types>> UserCalendars(Guid userId, bool userCreated = false)
        {
            var userCalendars = (
                await typeService.GetUserTypesAsync(userId))
                    .Where(x => x.GroupId == TypeGroup.Calendars && (userCreated && x.UserCreatedId == userId || !userCreated));

            foreach (var calendar in userCalendars)
            {
                calendar.InviteeName = (await GetByUserIDAsync(calendar.UserCreatedId)).Name;
            }

            return userCalendars;
        }

        public async Task<bool> UpdateAsync(User user)
        {
            user.ExtCalendarRights ??= Enumerable.Empty<ExtCalendarRights>();
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

        public async Task<bool> UpdateUserTagsAsync(IEnumerable<Tag> tags, Guid userId)
        {
            if (tags.Any())
            {
                var userTags = await GetUserTags(userId);
                var deletingTags = userTags.Where(x => x.UserID == userId).Select(x => x.Id).Except(tags.Select(x => x.Id));

                if (userTags.Any() && deletingTags.Any())
                {
                    foreach (var tag in deletingTags)
                    {
                        if (!await tagService.EventsByTagExist(tag))
                        {
                            await tagRepository.DeleteTagByIdAsync(tag);
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                return await tagRepository.UpdateUserTagsAsync(tags, userId);
            }
            else
            {
                await tagRepository.DeleteAllUserTagsAsync(userId);
                return true;
            }
        }

        public async Task<bool> RetainCalendarView(string view, Guid userId)
        {
            return await userRepository.RetainCalendarView(view, userId);
        }

        public async Task<User> GetUser(string email = null, string password = null)
        { 
            var user = await userRepository.GetAsync(email ?? SessionPersister.Email, password);

            if (user != null)
            {
                user.Authenticated = true;

                if (!user.EnableCronofy)
                {
                    user.CronofyReady = CronofyStatus.Disabled;
                }
                else if (string.IsNullOrEmpty(user.CronofyUid) || string.IsNullOrEmpty(user.AccessToken) || string.IsNullOrEmpty(user.RefreshToken))
                {
                    user.CronofyReady = CronofyStatus.NotAuthenticated;
                }
                else
                {
                    cronofyService.LoadUser(user);

                    var getCalendarNames = cronofyService.GetProfiles().Select(x => Utils.UppercaseFirst(x.ProviderName));
                    user.CronofyReadyCalendarName = string.Format("{0} Calendar{1}", string.Join(", ", getCalendarNames), getCalendarNames.Count() > 1 ? "s" : "");

                    if (user.ExtCalendarRights != null && user.ExtCalendarRights.Any(x => x.Read || x.Delete || x.Save))
                    {
                        user.CronofyReady = CronofyStatus.AuthenticatedRightsSet;
                    }
                    else
                    {
                        user.CronofyReady = CronofyStatus.AuthenticatedNoRightsSet;
                    }

                }

                return user;
            }

            return null;
        }

        public async Task<IEnumerable<Tag>> GetUserTags(Guid userId)
        {
            var user = await GetByUserIDAsync(userId);
            var userTags = await userRepository.GetTagsByUserAsync(userId);

            if (userTags != null && userTags.Any())
            {
                foreach (var tag in userTags)
                {
                    tag.UpdateDisabled = tag.UserID != user.UserID;
                }

                return userTags;
            }
 
            return Enumerable.Empty<Tag>();
        }



        public async Task<bool> RetainCalendarSelection(int[] calendarIds, Guid userId)
        {
            return await userRepository.RetainCalendarSelection(calendarIds, userId);
        }
    }
}
