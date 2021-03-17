
using Appology.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Appology.Repository;
using Appology.MiCalendar.Helpers;
using System.Web.Mvc;
using Appology.MiCalendar.Service;
using Appology.Write.Service;
using System;
using System.Web;
using Appology.Enums;

namespace Appology.Service
{
    public interface INotificationService
    {
        Task<IList<Notification>> DocumentNotifications(User user);
        Task<IList<Notification>> EventNotifications(Guid userId, int[] userCalendars);
    }

    public class NotificationService : INotificationService
    {
        private readonly IEventService eventService;
        private readonly ICategoryService categoryService;
        private readonly IDocumentService documentService;
        private readonly INotificationRepository notificationRepository;

        public NotificationService(
            IEventService eventService, 
            IDocumentService documentService, 
            ICategoryService categoryService,
            INotificationRepository notificationRepository)
        {
            this.eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
            this.documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
            this.categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            this.notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
        }


        public async Task<IList<Notification>> DocumentNotifications(User user)
        {
            var documentActivity = await documentService.RecentViewedDocs(user);
            return await GetNotifications(documentActivity, user.UserID, NotificationType.RecentlyViewedDocs);
        }

        public async Task<IList<Notification>> EventNotifications(Guid userId, int[] userCalendars)
        {
            var activeBuddyEvents = (await eventService.GetCurrentActivityAsync())
                .Where(x => userCalendars.Contains(x.CalendarId));

            var eventActivity = await eventService.EventActivity(activeBuddyEvents, userId);
            return await GetNotifications(eventActivity, userId, NotificationType.PresentAndUpcomingEvents);
        }

        private async Task<IList<Notification>> GetNotifications(IList<Notification> activity, Guid userId, NotificationType typeId)
        {
            var notifications = await notificationRepository.GetAllByUserIdAsync(userId, typeId);
            var addNotifications = activity.Where(ea => !notifications.Any(en => en.Id == ea.Id));
            var removeNotifications = notifications.Where(ea => !activity.Any(en => en.Id == ea.Id));

            if (addNotifications.Any(x => x.Id != Guid.Empty && x.UserId != Guid.Empty))
            {
                await InsertNotifications(addNotifications, typeId);
            }

            if (removeNotifications.Any(x => x.Id != Guid.Empty && x.UserId != Guid.Empty))
            {
                await notificationRepository.RemoveAsync(removeNotifications.Select(x => x.Id));
            }

            var url = new UrlHelper(HttpContext.Current.Request.RequestContext);

            return notifications
                .Select(x =>
                {
                    x.Avatar = CalendarUtils.AvatarSrc(x.UserId, x.Avatar, x.Name);
                    return x;
                })
                .ToList();
        }

        private async Task InsertNotifications(IEnumerable<Notification> notifications, NotificationType typeId)
        {
            if (notifications != null && notifications.Any())
            {
                foreach (var notification in notifications)
                {
                    await notificationRepository.InsertAsync(new Notification
                    {
                        Id = notification.Id,
                        TypeId = typeId,
                        HasRead = false,
                        Text = notification.Text,
                        UserId = notification.UserId
                    });
                }
            }
        }
    }
}
