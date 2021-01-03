using Dapper;
using DFM.Utils;
using MyCalendar.DTOs;
using MyCalendar.Enums;
using MyCalendar.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace MyCalendar.Repository
{
    public interface IUserRepository
    {
        Task<User> GetAsync(string email, string password = null);
        Task<IEnumerable<User>> GetAllAsync();
        Task<bool> UpdateAsync(User user);
        Task<IEnumerable<Tag>> GetTagsByUserAsync(Guid userID);
        Task<User> GetByUserIDAsync(Guid userID);
        User Get(string email);
        Task<bool> CronofyAccountRequest(string accessToken, string refreshToken, string cronofyUid);
        Task<bool> RetainCalendarSelection(int[] calendarIds, Guid userId);
        Task<bool> UpdateLastViewedDoc(Guid userId, Guid docId);
        Task<bool> RetainCalendarView(string view, Guid userId);
    }

    public class UserRepository : IUserRepository
    {
        private readonly Func<IDbConnection> dbConnectionFactory;
        private static readonly string TABLE = "Users";
        private static readonly string[] FIELDS = typeof(User).DapperFields();
        private static readonly string[] TAGSFIELDS = typeof(Tag).DapperFields();

        public UserRepository(Func<IDbConnection> dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
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
                             LastViewedDocId = x.LastViewedDocId,
                             SelectedCalendars = x.SelectedCalendars,
                             SelectedCalendarView = x.SelectedCalendarView,
                             ExtCalendars = x.ExtCalendars,
                             ExtCalendarRights = x.ExtCalendars != null ? JsonConvert.DeserializeObject<IEnumerable<ExtCalendarRights>>(x.ExtCalendars) : null,
                         })
                        .FirstOrDefault();
            }
        }

        public async Task<User> GetByUserIDAsync(Guid userID)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<User>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE UserID = @userID", new { userID })).FirstOrDefault();
            }
        }

        public User Get(string email)
        {
            using (var sql = dbConnectionFactory())
            {
                return (sql.Query<User>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE Email = @email", new { email })).FirstOrDefault();
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
                FROM Events AS e
                RIGHT JOIN Tags AS t
                ON e.TagID = t.Id
                WHERE t.UserId = '{userID}'
                GROUP BY t.Id, t.UserID, t.TypeID, t.Name, t.ThemeColor
                ORDER BY Count DESC
            ";

            using var sql = dbConnectionFactory();
            return (await sql.QueryAsync<Tag>(sqlTxt)).ToArray();
        }


        public async Task<bool> UpdateAsync(User user)
        {
            using var sql = dbConnectionFactory();
            try
            {
                await sql.ExecuteAsync($"{DapperHelper.UPDATE(TABLE, FIELDS, "")} WHERE UserID = @UserId",
                    new
                    {
                        user.UserID,
                        user.Name,
                        user.Email,
                        user.Password,
                        user.PhoneNumber,
                        user.CronofyUid,
                        user.AccessToken,
                        user.RefreshToken,
                        user.EnableCronofy,
                        user.RoleIds,
                        user.BuddyIds,
                        user.Avatar,
                        user.LastViewedDocId,
                        user.SelectedCalendars,
                        user.SelectedCalendarView,

                        ExtCalendars = user.ExtCalendarRights.Any() ? JsonConvert.SerializeObject(user.ExtCalendarRights) : user.ExtCalendars
                    });

                return true;
            }
            catch (Exception exp)
            {
                string.IsNullOrEmpty(exp.Message);
                return false;
            }
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
                    string.IsNullOrEmpty(exp.Message);
                    return false;
                }
            }
        }

        public async Task<bool> RetainCalendarSelection(int[] calendarIds, Guid userId)
        {
            using var sql = dbConnectionFactory();
            try
            {
                await sql.ExecuteAsync($"UPDATE {TABLE} SET SelectedCalendars = @SelectedCalendars WHERE UserID = @UserId",
                    new
                    {
                        SelectedCalendars = calendarIds != null && calendarIds.Any() ? string.Join(",", calendarIds) : null,
                        UserId = userId
                    });

                return true;
            }
            catch (Exception exp)
            {
                string.IsNullOrEmpty(exp.Message);
                return false;
            }
        }

        public async Task<bool> RetainCalendarView(string view, Guid userId)
        {
            using var sql = dbConnectionFactory();
            try
            {
                await sql.ExecuteAsync($"UPDATE {TABLE} SET SelectedCalendarView = @View WHERE UserID = @UserId",
                    new
                    {
                        View = view,
                        UserId = userId
                    });

                return true;
            }
            catch (Exception exp)
            {
                string.IsNullOrEmpty(exp.Message);
                return false;
            }
        }

        public async Task<bool> UpdateLastViewedDoc(Guid userId, Guid docId)
        {
            using (var sql = dbConnectionFactory())
            {
                try
                {
                    await sql.ExecuteAsync($"UPDATE {TABLE} SET LastViewedDocId = @docId WHERE UserID = @userId", new { docId,userId });
                    return true;
                }
                catch (Exception exp)
                {
                    string.IsNullOrEmpty(exp.Message);
                    return false;
                }
            }
        }
    }
}
