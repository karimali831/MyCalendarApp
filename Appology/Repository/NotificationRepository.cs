using DFM.Utils;
using Appology.Enums;
using Appology.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Appology.Repository
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetAllByUserIdAsync(Guid userId, NotificationType typeId);
        Task<bool> InsertAsync(Notification notification);
        Task<bool> RemoveAsync(IEnumerable<Guid> ids);
    }

    public class NotificationRepository : DapperBaseRepository, INotificationRepository
    {
        private static readonly string TABLE = Tables.Name(Table.Notifications);
        private static readonly string[] FIELDS = typeof(Notification).DapperFields();

        public NotificationRepository(Func<IDbConnection> dbConnectionFactory) : base(dbConnectionFactory) { }

        public async Task<IEnumerable<Notification>> GetAllByUserIdAsync(Guid userId, NotificationType typeId)
        {
            return await QueryAsync<Notification>($@"
                SELECT n.Id, n.UserId, n.Text, n.HasRead, u.Avatar, u.Name, f.FaIcon
                FROM {TABLE} AS n
                LEFT JOIN {Tables.Name(Table.Users)} AS u
                ON n.UserId = u.UserID
                LEFT JOIN {Tables.Name(Table.Categories)} AS c
				ON n.TypeId = c.Id
				LEFT JOIN {Tables.Name(Table.Features)}  AS f
				ON f.Id = c.FeatureId
                WHERE n.UserId = @userId
                AND TypeId = @typeId", new { userId, typeId = (int)typeId });
        }

        public async Task<bool> InsertAsync(Notification notification)
        {
            return await ExecuteAsync($"{DapperHelper.INSERT(TABLE, FIELDS)}", notification);
        }

        public async Task<bool> RemoveAsync(IEnumerable<Guid> ids)
        {
            return await ExecuteAsync($"{DapperHelper.DELETE(TABLE)} WHERE Id IN @ids ", new { ids } );
        }
    }
}
