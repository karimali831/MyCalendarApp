using Dapper;
using DFM.Utils;
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
        Task<bool> UpdateAsync(User e);
    }

    public class UserRepository : IUserRepository
    {
        private readonly Func<IDbConnection> dbConnectionFactory;
        private static readonly string TABLE = "Users";
        private static readonly string[] FIELDS = typeof(User).DapperFields();

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

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<User>($"{DapperHelper.SELECT(TABLE, FIELDS)}")).ToArray();
            }
        }

        public async Task<bool> UpdateAsync(User e)
        {
            using (var sql = dbConnectionFactory())
            {
                try
                {
                    await sql.ExecuteAsync($"{DapperHelper.UPDATE(TABLE, FIELDS, "")} WHERE Passcode = @passcode", 
                        new
                        {
                            userId = e.UserID,
                            name = e.Name,
                            email = e.Email,
                            passcode = e.Passcode,
                            phoneNumber = e.PhoneNumber
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
