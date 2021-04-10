using DFM.Utils;
using Appology.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Appology.MiCalendar.Model;
using Appology.Enums;
using Appology.DTOs;
using Appology.Write.DTOs;

namespace Appology.Repository
{
    public interface IUserRepository
    {
        Task<User> GetAsync(string email, string password = null);
        Task<IEnumerable<User>> GetAllAsync();
        Task<IEnumerable<Tag>> GetTagsByUserAsync(Guid userID);
        Task<User> GetByUserIDAsync(Guid userID);
        Task<bool> CronofyAccountRequest(string accessToken, string refreshToken, string cronofyUid);
        Task<bool> UpdateRecentOpenedDocs(Guid userId, string docIds);
        Task<bool> PinDoc(Guid userId, string docIds);
        Task<bool> SaveUserInfo(UserInfoDTO dto);
        Task<bool> SaveCalendarSettings(CalendarSettingsDTO dto);
        Task<bool> GroupExistsInTag(int groupId);
        Task<bool> UpdateBuddys(string buddys, Guid userId);
        Task<IList<Collaborator>> GetCollaboratorsAsync(IEnumerable<Guid> inviteeIds);
        Task<bool> UpdateCronofyUserCredentials(string cronofyUid, string accessToken, string refreshToken, Guid userId);
        Task<bool> UpdateCronofyCalendarRights(IEnumerable<ExtCalendarRights> rights, Guid userId);
    }

    public class UserRepository : DapperBaseRepository, IUserRepository
    {
        private static readonly string TABLE = Tables.Name(Table.Users);
        private static readonly string[] FIELDS = typeof(User).DapperFields();


        public UserRepository(Func<IDbConnection> dbConnectionFactory) : base(dbConnectionFactory) { }

        public async Task<User> GetAsync(string email, string password = null)
        {
            var user = await QueryFirstOrDefaultAsync<User>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE Email = @email {(password != null ? $"AND Password = '{password}'" : "")} ", new { email });

            if (user == null)
            {
                return null;
            }

            return new User
            {
                UserID = user.UserID,
                Password = user.Password,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CronofyUid = user.CronofyUid,
                AccessToken = user.AccessToken,
                RefreshToken = user.RefreshToken,
                EnableCronofy = user.EnableCronofy,
                BuddyIds = user.BuddyIds,
                RoleIds = user.RoleIds,
                Avatar = user.Avatar,
                RecentOpenedDocIds = user.RecentOpenedDocIds,
                PinnedDocIds = user.PinnedDocIds,
                SelectedCalendars = user.SelectedCalendars,
                DefaultCalendarView = user.DefaultCalendarView,
                DefaultNativeCalendarView = user.DefaultNativeCalendarView,
                ExtCalendars = user.ExtCalendars,
                ExtCalendarRights = user.ExtCalendars != null ? JsonConvert.DeserializeObject<IEnumerable<ExtCalendarRights>>(user.ExtCalendars) : null,
            };   
        }
        public async Task<bool> SaveUserInfo(UserInfoDTO dto)
        {
            return await ExecuteAsync($"{DapperHelper.UPDATE(TABLE, typeof(UserInfoDTO).DapperFields(), "")} WHERE UserID = @UserID", dto);
        }

        public async Task<bool> SaveCalendarSettings(CalendarSettingsDTO dto)
        {
            return await ExecuteAsync($"{DapperHelper.UPDATE(TABLE, typeof(CalendarSettingsDTO).DapperFields(), "")} WHERE UserID = @UserID", dto);
        }

        public async Task<User> GetByUserIDAsync(Guid userID)
        {
            return await QueryFirstOrDefaultAsync<User>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE UserID = @userID", new { userID });
        }

        public async Task<IList<Collaborator>> GetCollaboratorsAsync(IEnumerable<Guid> inviteeIds)
        {
            return await QueryAsync<Collaborator>($"SELECT UserId AS CollaboratorId, Avatar, Name FROM {TABLE} WHERE UserID IN @inviteeIds", new { inviteeIds });
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await QueryAsync<User>($"{DapperHelper.SELECT(TABLE, FIELDS)}");
        }

        public async Task<IEnumerable<Tag>> GetTagsByUserAsync(Guid userID)
        {
            string sqlTxt = $@"
                SELECT t.Id, t.UserID, t.TypeID, t.Name, t.ThemeColor, COUNT(*) AS Count
                FROM {Tables.Name(Table.Events)} AS e
                RIGHT JOIN {Tables.Name(Table.Tags)} AS t
                ON e.TagID = t.Id
                WHERE t.UserId = '{userID}'
                GROUP BY t.Id, t.UserID, t.TypeID, t.Name, t.ThemeColor
                ORDER BY Count DESC
            ";

            return await QueryAsync<Tag>(sqlTxt);
        }

        public async Task<bool> CronofyAccountRequest(string accessToken, string refreshToken, string cronofyUid)
        {
            return await ExecuteAsync($"UPDATE {TABLE} SET AccessToken = @accessToken, RefreshToken = @refreshToken WHERE CronofyUid = @cronofyUid",
                new
                {
                    accessToken,
                    refreshToken,
                    cronofyUid
                });
        }

        public async Task<bool> UpdateRecentOpenedDocs(Guid userId, string docIds)
        {
            return await ExecuteAsync($"UPDATE {TABLE} SET RecentOpenedDocIds = @docIds WHERE UserID = @userId", new { docIds ,userId });
 
        }

        public async Task<bool> PinDoc(Guid userId, string docIds)
        {
            return await ExecuteAsync($"UPDATE {TABLE} SET PinnedDocIds = @docIds WHERE UserID = @userId", new { docIds, userId });
        }

        public async Task<bool> GroupExistsInTag(int groupId)
        {
            return await ExecuteScalarAsync<bool>($"SELECT count(1) FROM {Tables.Name(Table.Tags)} WHERE TypeId = @groupId", new { groupId });
        }

        public async Task<bool> UpdateBuddys(string buddys, Guid userId)
        {
            return await ExecuteAsync($"UPDATE {TABLE} SET BuddyIds = @buddys WHERE UserID = @userId", new { buddys, userId });
        }

        public async Task<bool> UpdateCronofyUserCredentials(string cronofyUid, string accessToken, string refreshToken, Guid userId)
        {
            return await ExecuteAsync($"UPDATE {TABLE} SET CronofyUid = @cronofyUid, AccessToken = @accessToken, RefreshToken = @refreshToken WHERE UserID = @userId", 
                new { 
                    cronofyUid,
                    accessToken,
                    refreshToken,
                    userId 
                });    
        }


        public async Task<bool> UpdateCronofyCalendarRights(IEnumerable<ExtCalendarRights> rights, Guid userId)
        {
            return await ExecuteAsync($"UPDATE {TABLE} SET ExtCalendars = @ExtCalendars WHERE UserID = @UserId",
                new
                {
                    ExtCalendars = rights != null && rights.Any() ? JsonConvert.SerializeObject(rights) : null,
                    UserId = userId
                });
        }
    }
}
