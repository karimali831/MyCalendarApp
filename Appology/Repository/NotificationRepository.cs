using Dapper;
using DFM.Utils;
using Appology.Enums;
using Appology.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using DFM.ExceptionHandling.Sentry;
using System.Configuration;
using DFM.ExceptionHandling;

namespace Appology.Repository
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetAllByUserIdAsync(Guid userId, Features featureId);
        Task<bool> InsertAsync(Notification notification);
        Task<bool> RemoveAsync(IEnumerable<Guid> ids);
    }

    public class NotificationRepository : INotificationRepository
    {
        private readonly Func<IDbConnection> dbConnectionFactory;
        private readonly IExceptionHandlerService exceptionHandlerService;
        private static readonly string TABLE = Tables.Name(Table.Notifications);
        private static readonly string[] FIELDS = typeof(Notification).DapperFields();

        public NotificationRepository(Func<IDbConnection> dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
            this.exceptionHandlerService = new ExceptionHandlerService(ConfigurationManager.AppSettings["DFM.ExceptionHandling.Sentry.Environment"]);
        }

        public async Task<IEnumerable<Notification>> GetAllByUserIdAsync(Guid userId, Features featureId)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Notification>($@"
                    SELECT n.Id, n.UserId, n.Text, n.HasRead, u.Avatar, u.Name
                    FROM {TABLE} AS n
                    LEFT JOIN {Tables.Name(Table.Users)} AS u
                    ON n.UserId = u.UserID
                    WHERE n.UserId = @userId
                    AND FeatureId = @featureId", new { userId, featureId = (int)featureId })).ToArray();
            }
        }

        public async Task<bool> InsertAsync(Notification notification)
        {
            using (var sql = dbConnectionFactory())
            {
                try
                {
                    await sql.ExecuteAsync($"{DapperHelper.INSERT(TABLE, FIELDS)}", notification);
                    return true;
                }
                catch (Exception exp)
                {
                    exceptionHandlerService.ReportException(exp).Submit();
                    return false;
                }

            }
        }

        public async Task<bool> RemoveAsync(IEnumerable<Guid> ids)
        {
            using (var sql = dbConnectionFactory())
            {
                try
                {
                    await sql.ExecuteAsync($"{DapperHelper.DELETE(TABLE)} WHERE Id IN @ids ", new { ids } );
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
}
