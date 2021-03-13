using Dapper;
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
using DFM.ExceptionHandling;
using DFM.ExceptionHandling.Sentry;
using System.Configuration;
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

    public class UserRepository : IUserRepository
    {
        private readonly Func<IDbConnection> dbConnectionFactory;
        private static readonly string TABLE = Tables.Name(Table.Users);
        private readonly IExceptionHandlerService exceptionHandlerService;
        private static readonly string[] FIELDS = typeof(User).DapperFields();


        public UserRepository(Func<IDbConnection> dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
            this.exceptionHandlerService = new ExceptionHandlerService(ConfigurationManager.AppSettings["DFM.ExceptionHandling.Sentry.Environment"]);
        }

        public async Task<User> GetAsync(string email, string password = null)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<User>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE Email = @email {(password != null ? $"AND Password = '{password}'" : "")} ", new { email } ))
                    .Select(x => new User
                        {
                            UserID = x.UserID,
                            Password = x.Password,
                            Name = x.Name,
                            Email = x.Email,
                            PhoneNumber = x.PhoneNumber,
                            CronofyUid = x.CronofyUid,
                            AccessToken = x.AccessToken,
                            RefreshToken = x.RefreshToken,
                            EnableCronofy = x.EnableCronofy,
                            BuddyIds = x.BuddyIds,
                            RoleIds = x.RoleIds,
                            Avatar = x.Avatar,
                            RecentOpenedDocIds = x.RecentOpenedDocIds,
                            PinnedDocIds = x.PinnedDocIds,
                            SelectedCalendars = x.SelectedCalendars,
                            DefaultCalendarView = x.DefaultCalendarView,
                            DefaultNativeCalendarView = x.DefaultNativeCalendarView,
                            ExtCalendars = x.ExtCalendars,
                            ExtCalendarRights = x.ExtCalendars != null ? JsonConvert.DeserializeObject<IEnumerable<ExtCalendarRights>>(x.ExtCalendars) : null,
                        })
                    .FirstOrDefault();
            }
        }

        public async Task<bool> SaveUserInfo(UserInfoDTO dto)
        {
            using (var sql = dbConnectionFactory())
            {
                try
                {
                    await sql.ExecuteAsync($"{DapperHelper.UPDATE(TABLE, typeof(UserInfoDTO).DapperFields(), "")} WHERE UserID = @UserID", dto);
                    return true;
                }
                catch (Exception exp)
                {
                    exceptionHandlerService.ReportException(exp).Submit();
                    return false;
                }
            }
        }

        public async Task<bool> SaveCalendarSettings(CalendarSettingsDTO dto)
        {
            using (var sql = dbConnectionFactory())
            {
                try
                {
                    await sql.ExecuteAsync($"{DapperHelper.UPDATE(TABLE, typeof(CalendarSettingsDTO).DapperFields(), "")} WHERE UserID = @UserID", dto);
                    return true;
                }
                catch (Exception exp)
                {
                    exceptionHandlerService.ReportException(exp).Submit();
                    return false;
                }
            }
        }

        public async Task<User> GetByUserIDAsync(Guid userID)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<User>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE UserID = @userID", new { userID })).FirstOrDefault();
            }
        }

        public async Task<IList<Collaborator>> GetCollaboratorsAsync(IEnumerable<Guid> inviteeIds)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Collaborator>($"SELECT UserId AS CollaboratorId, Avatar, Name FROM {TABLE} WHERE UserID IN @inviteeIds", new { inviteeIds })).ToList();
            }
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<User>($"{DapperHelper.SELECT(TABLE, FIELDS)}")).ToArray();
            }
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

            using var sql = dbConnectionFactory();
            return (await sql.QueryAsync<Tag>(sqlTxt)).ToArray();
        }

        public async Task<bool> CronofyAccountRequest(string accessToken, string refreshToken, string cronofyUid)
        {
            using (var sql = dbConnectionFactory())
            {
                try
                {
                    await sql.ExecuteAsync($"UPDATE {TABLE} SET AccessToken = @accessToken, RefreshToken = @refreshToken WHERE CronofyUid = @cronofyUid",
                        new
                        {
                            accessToken,
                            refreshToken,
                            cronofyUid
                        });

                    return true;
                }
                catch (Exception exp)
                {
                    exceptionHandlerService.ReportException(exp).Submit();
                    return false;
                }
            }
        }

        public async Task<bool> UpdateRecentOpenedDocs(Guid userId, string docIds)
        {
            using (var sql = dbConnectionFactory())
            {
                await sql.ExecuteAsync($"UPDATE {TABLE} SET RecentOpenedDocIds = @docIds WHERE UserID = @userId", new { docIds ,userId });
                return true;

            }
        }

        public async Task<bool> PinDoc(Guid userId, string docIds)
        {
            using (var sql = dbConnectionFactory())
            {
                await sql.ExecuteAsync($"UPDATE {TABLE} SET PinnedDocIds = @docIds WHERE UserID = @userId", new { docIds, userId });
                return true;
            }
        }

        public async Task<bool> GroupExistsInTag(int groupId)
        {
            using (var sql = dbConnectionFactory())
            {
                return await sql.ExecuteScalarAsync<bool>($"SELECT count(1) FROM {Tables.Name(Table.Tags)} WHERE TypeId = @groupId", new { groupId });
            }
        }

        public async Task<bool> UpdateBuddys(string buddys, Guid userId)
        {
            using (var sql = dbConnectionFactory())
            {
                try
                {
                    await sql.ExecuteAsync($"UPDATE {TABLE} SET BuddyIds = @buddys WHERE UserID = @userId", new { buddys, userId });
                    return true;
                }
                catch (Exception exp)
                {
                    exceptionHandlerService.ReportException(exp).Submit();
                    return false;
                }
            }
        }

        public async Task<bool> UpdateCronofyUserCredentials(string cronofyUid, string accessToken, string refreshToken, Guid userId)
        {
            using(var sql = dbConnectionFactory())
            {
                try
                {
                    await sql.ExecuteAsync($"UPDATE {TABLE} SET CronofyUid = @cronofyUid, AccessToken = @accessToken, RefreshToken = @refreshToken WHERE UserID = @userId", 
                        new { 
                            cronofyUid,
                            accessToken,
                            refreshToken,
                            userId 
                        });

                    return true;
                }
                catch (Exception exp)
                {
                    exceptionHandlerService.ReportException(exp).Submit();
                    return false;
                }
            }
        }


        public async Task<bool> UpdateCronofyCalendarRights(IEnumerable<ExtCalendarRights> rights, Guid userId)
        {
            using var sql = dbConnectionFactory();
            try
            {
                await sql.ExecuteAsync($"UPDATE {TABLE} SET ExtCalendars = @ExtCalendars WHERE UserID = @UserId",
                    new
                    {
                        ExtCalendars = rights != null && rights.Any() ? JsonConvert.SerializeObject(rights) : null,
                        UserId = userId
                    });

                return true;
            }
            catch (Exception exp)
            {
                exceptionHandlerService.ReportException(exp).Submit();
                return false;
            }
        }
    }
}
