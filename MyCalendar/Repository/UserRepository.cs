using Dapper;
using DFM.Utils;
using MyCalendar.DTOs;
using MyCalendar.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace MyCalendar.Repository
{
    public interface IUserRepository
    {
        Task<User> GetAsync(int passcode);
        Task<IEnumerable<User>> GetAllAsync();
        Task<bool> UpdateAsync(User user);
        Task<User> GetByUserIDAsync(Guid userID);
        User GetByCronofyIDAsync(string cronofyUid);
        Task<bool> CronofyAccountRequest(string accessToken, string refreshToken, string cronofyUid);
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

        public async Task<User> GetAsync(int passcode)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<User>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE Passcode = @passcode", new { passcode })).FirstOrDefault();
            }
        }

        public async Task<User> GetByUserIDAsync(Guid userID)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<User>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE UserID = @userID", new { userID })).FirstOrDefault();
            }
        }

        public User GetByCronofyIDAsync(string cronofyUid)
        {
            using (var sql = dbConnectionFactory())
            {
                return (sql.Query<User>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE CronofyUid = @cronofyUid", new { cronofyUid })).FirstOrDefault();
            }
        }


        public async Task<IEnumerable<User>> GetAllAsync()
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<User>($"{DapperHelper.SELECT(TABLE, FIELDS)}")).ToArray();
            }
        }

        public async Task<bool> UpdateAsync(User user)
        {
            using (var sql = dbConnectionFactory())
            {
                try
                {
                    await sql.ExecuteAsync($"{DapperHelper.UPDATE(TABLE, FIELDS, "")} WHERE UserID = @userId", 
                        new
                        {
                            user.UserID,
                            user.Name,
                            user.Email,
                            user.Passcode,
                            user.PhoneNumber,
                            user.CronofyUid,
                            user.AccessToken,
                            user.RefreshToken
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

        public async Task<bool> CronofyAccountRequest(string accessToken, string refreshToken, string cronofyUid)
        {
            using (var sql = dbConnectionFactory())
            {
                try
                {
                    await sql.ExecuteAsync($"{DapperHelper.UPDATE(TABLE, FIELDS, "")} WHERE CronofyUID=@cronofyUid",
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
    }
}
