using Appology.Helpers;
using Appology.MiCalendar.Service;
using Appology.Model;
using Appology.Write.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Appology.Service
{
    public interface INotificationService
    {
        Task<IList<Notification>> GetNotifications(User user, int[] userCalendars);
    }

    public class NotificationService : INotificationService
    {
        private readonly IEventService eventService;
        private readonly IDocumentService documentService;

        public NotificationService(IEventService eventService, IDocumentService documentService)
        {
            this.eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
            this.documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
        }

        public async Task<IList<Notification>> GetNotifications(User user, int[] userCalendars)
        {
            var activeBuddyEvents = (await eventService.GetCurrentActivityAsync())
                .Where(x => userCalendars.Contains(x.CalendarId));

            var eventActivity = await eventService.EventActivity(activeBuddyEvents, user.UserID);
            var documentActivity = await documentService.DocumentActivity(user);

            return (eventActivity
                .Concat(documentActivity))
                .ToList();
        }
    }
}
