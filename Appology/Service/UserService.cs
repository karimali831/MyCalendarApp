using Appology.Helpers;
using Appology.Model;
using Appology.Repository;
using Appology.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Appology.Security;
using Appology.MiCalendar.Model;
using Appology.MiCalendar.Service;
using Appology.MiCalendar.Repository;
using Appology.MiCalendar.Enums;
using Appology.DTOs;
using Appology.MiCalendar.DTOs;

namespace Appology.Service
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User> GetUser(string email = null, string password = null);
        Task<bool> UpdateAsync(User user);
        Task<User> GetByUserIDAsync(Guid userID);
        Task<bool> UpdateUserTagsAsync(IList<Tag> tags, Guid userId);
        Task<Tag> GetUserTagAysnc(Guid tagID);
        Task<IList<User>> GetBuddys(Guid userId);
        Task<IEnumerable<Tag>> GetUserTags(Guid userId);
        Task<IEnumerable<Types>> UserCalendars(Guid userId, bool userCreated = false);
        Task<bool> SaveUserInfo(UserInfoDTO dto);
        Task<bool> SaveCalendarSettings(CalendarSettingsDTO dto);
        Task<(bool Status, Types UserType)> SaveUserType(UserTypeDTO dto);
        Task<(bool Status, string Msg)> DeleteUserType(int Id, Guid userId);
        Task<bool> GroupExistsInTag(int groupId);
        Task<bool> UpdateBuddys(string buddys, Guid userId);
        Task<(Status? UpdateResponse, string UpdateMsg)> AddBuddy(string email, Guid id);
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
                await typeService.GetUserTypesAsync(userId, TypeGroup.Calendars))
                    .Where(x => userCreated && x.UserCreatedId == userId || !userCreated);

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

        public async Task<bool> UpdateUserTagsAsync(IList<Tag> tags, Guid userId)
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

        public async Task<bool> SaveUserInfo(UserInfoDTO dto)
        {
            return await userRepository.SaveUserInfo(dto);
        }

        public async Task<bool> SaveCalendarSettings(CalendarSettingsDTO dto)
        {
            return await userRepository.SaveCalendarSettings(dto);
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

        public async Task<(bool Status, Types UserType)> SaveUserType(UserTypeDTO dto)
        {
            if (dto.Id != 0)
            {
                var userType = new Types
                {
                    Id = dto.Id.Value,
                    GroupId = dto.GroupId,
                    Name = dto.Name,
                    InviteeIds = dto.InviteeIds,
                    SuperTypeId = dto.SuperTypeId,
                    UserCreatedId = dto.UserCreatedId
                };

                var update = await typeService.UpdateTypeAsync(userType);

                return (update, userType);
            }
            else
            {
                return await typeService.AddTypeAsync(new TypeDTO
                {
                    GroupId = dto.GroupId,
                    Name = dto.Name,
                    InviteeIds = dto.InviteeIds,
                    SuperTypeId = dto.SuperTypeId,
                    UserCreatedId = dto.UserCreatedId
                });
            }
        }

        public async Task<(bool Status, string Msg)> DeleteUserType(int Id, Guid userId)
        {
            return await typeService.DeleteTypeAsync(Id, userId);
        }

        public async Task<bool> GroupExistsInTag(int groupId)
        {
            return await userRepository.GroupExistsInTag(groupId);
        }

        public async Task<bool> UpdateBuddys(string buddys, Guid userId)
        {
            return await userRepository.UpdateBuddys(buddys, userId);
        }

        public async Task<(Status? UpdateResponse, string UpdateMsg)> AddBuddy(string email, Guid id)
        {
            var user = await GetUser(email);

            (Status? UpdateResponse, string UpdateMsg) status = (null, null);

            if (user == null || id == Guid.Empty)
            {
                status.UpdateResponse = Status.Failed;
                status.UpdateMsg = "Invalid request";
            }
            else if (user.UserID == id)
            {
                status.UpdateResponse = Status.Failed;
                status.UpdateMsg = "You cannot add yourself as a buddy";
            }
            else if (!(await GetAllAsync()).Any(x => x.UserID == id))
            {
                status.UpdateResponse = Status.Failed;
                status.UpdateMsg = "Could not find user";
            }
            else
            {
                var buddys = await GetBuddys(user.UserID);
                var inviter = await GetByUserIDAsync(id);
                var inviterBuddyList = await GetBuddys(id);

                if (buddys != null && buddys.Any(x => x.UserID == id))
                {
                    status.UpdateResponse = Status.Failed;
                    status.UpdateMsg = $"{inviter.Name} is already on your buddy-list";
                }
                else if (inviterBuddyList != null && inviterBuddyList.Any(x => x.UserID == id))
                {
                    status.UpdateResponse = Status.Failed;
                    status.UpdateMsg = $"You are already on {inviter.Name}'s buddy-list";
                }
                else
                {
                    var inviteeBuddyIds = new List<Guid>();

                    // add to invitees buddy list
                    if (!string.IsNullOrEmpty(user.BuddyIds))
                    {
                        inviteeBuddyIds.Add(id);
                        user.BuddyIds = string.Join(",", inviteeBuddyIds.Concat(buddys.Select(x => x.UserID)));
                    }
                    else
                    {
                        user.BuddyIds = id.ToString();
                    }

                    var inviterBuddyIds = new List<Guid>();

                    // to add inviters buddy-list
                    if (!string.IsNullOrEmpty(inviter.BuddyIds))
                    {
                        inviterBuddyIds.Add(user.UserID);
                        inviter.BuddyIds = string.Join(",", inviterBuddyIds.Concat(inviterBuddyList.Select(x => x.UserID)));
                    }
                    else
                    {
                        inviter.BuddyIds = user.UserID.ToString();
                    }

                    var updateInvitee = await UpdateBuddys(user.BuddyIds, user.UserID);
                    var updateInviter = await UpdateBuddys(inviter.BuddyIds, inviter.UserID);

                    status = (updateInvitee && updateInviter)
                        ? (Status.Success, $"You and {inviter.Name} are now buddys")
                        : (Status.Failed, $"There was an issue adding you or {inviter.Name}'s as a buddy");
                }
            }

            return status;
        }
    }
}
