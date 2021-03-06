﻿using Appology.MiCalendar.DTOs;
using Appology.MiCalendar.Enums;
using Appology.MiCalendar.Model;
using Appology.MiCalendar.Repository;
using Appology.DTOs;
using Appology.Enums;
using Appology.Helpers;
using Appology.Model;
using Appology.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Appology.MiCalendar.Helpers;
using System.Web.Helpers;

namespace Appology.MiCalendar.Service
{
    public interface IEventService
    {
        Task<Event> GetAsync(Guid eventId);
        Task<IEnumerable<Event>> GetEventsAsync(RequestEventDTO request, bool cacheRemove = false);
        Task<IEnumerable<Event>> GetAllAsync(User user, RequestEventDTO request);
        Task<bool> SaveEvent(Event dto);
        Task<Event> SaveGetEvent(Event dto);
        Task<bool> DeleteEvent(Guid eventId, string eventUid = null);
        Task<IEnumerable<Event>> GetCurrentActivityAsync();
        Task<bool> EventExistsInCalendar(int calendarId);
        Task<IEnumerable<Types>> GetAccessibleCalendars(Guid userId);
        Task<IEnumerable<Types>> GetUserCalendars(Guid userId);
        Task<bool> EventExistsAtStartTime(DateTime startDate, int calendarId);
        Task<string> GetLastStoredAlarm(Guid tagId);
        Task SaveCronofyEvent(Event e, IEnumerable<ExtCalendarRights> Rights);
        void DeleteCronofyEvent(string syncFromCalendarId, Guid eventId);
        Task<IList<Notification>> EventActivity(IEnumerable<Event> events, Guid userId);
    }

    public class EventService : IEventService
    {
        public static readonly string cachePrefix = typeof(EventService).FullName;
        private readonly IEventRepository eventRepository;
        private readonly ICronofyService cronofyService;
        private readonly IUserService userService;
        private readonly ITypeService typeService;
        private readonly ICacheService cache;

        public EventService(
            IEventRepository eventRepository, 
            ICronofyService cronofyService, 
            ITypeService typeService,
            IUserService userService,
            ICacheService cache)
        {
            this.eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
            this.cronofyService = cronofyService ?? throw new ArgumentNullException(nameof(Cronofy));
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.typeService = typeService ?? throw new ArgumentNullException(nameof(typeService));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<Event> GetAsync(Guid eventId)
        {
            return await cache.GetAsync(
                $"{cachePrefix}.{nameof(GetAsync)}",
                async () => await eventRepository.GetAsync(eventId)
            );
        }

        public async Task<IEnumerable<Event>> GetCurrentActivityAsync()
        {
            return await cache.GetAsync(
                $"{cachePrefix}.{nameof(GetCurrentActivityAsync)}",
                async () =>
                {
                    return (await eventRepository.GetCurrentActivityAsync())
                        .Where(x => (DateUtils.DateTime() >= DateUtils.FromUtcToTimeZone(x.StartDate.AddHours(-4)) && x.EndDate.HasValue && DateUtils.DateTime() < DateUtils.FromUtcToTimeZone(x.EndDate.Value)) ||
                                (!x.EndDate.HasValue && DateUtils.FromUtcToTimeZone(x.StartDate).Date == DateUtils.DateTime().Date));
                }
            );
        }

        public async Task<IEnumerable<Types>> GetAccessibleCalendars(Guid userId)
        {
            return await typeService.GetUserTypesAsync(userId, TypeGroup.Calendars);
        }

        public async Task<IEnumerable<Types>> GetUserCalendars(Guid userId)
        {
            return await typeService.GetAllByUserIdAsync(userId, TypeGroup.Calendars);
        }

        public async Task<string> GetLastStoredAlarm(Guid tagId)
        {
            return await eventRepository.GetLastStoredAlarm(tagId);
        }

        public async Task<IList<Notification>> EventActivity(IEnumerable<Event> events, Guid userId)
        {
            var currentActivity = new List<Notification>();

            if (events != null && events.Any())
            {
                foreach (var activity in events)
                {
                    var user = await userService.GetByUserIDAsync(activity.UserID);
                    string getName = "";

                    if (activity.InviteeIdsList.Any())
                    {
                        var inviteeList = new List<string>();
                        foreach (var invitee in activity.InviteeIdsList)
                        {
                            string inviteeName;
                            if (invitee != userId)
                            {
                                inviteeName = (await userService.GetByUserIDAsync(invitee)).Name;
                            }
                            else
                            {
                                inviteeName = user.Name;
                            }
                            inviteeList.Add(inviteeName);
                        }

                        getName += $"You, {string.Join(", ", string.Join(", ", inviteeList))}";
                    }
                    else
                    {
                        getName = (userId == activity.UserID ? "You" : user.Name);
                    }

                    string label = activity.Subject ?? activity.Description;
                    string finishing = (activity.EndDate.HasValue ? "finishing " + DateUtils.FromUtcToTimeZone(activity.EndDate.Value).ToString("HH:mm") : "for the day");
                    string starting =  activity.StartDate.ToString("HH:mm");
                    string avatar = CalendarUtils.AvatarSrc(user.UserID, user.Avatar, user.Name);

                    if (DateUtils.DateTime() >= DateUtils.FromUtcToTimeZone(activity.StartDate.AddHours(-4)) && DateUtils.DateTime() < DateUtils.FromUtcToTimeZone(activity.StartDate))
                    {
                        string pronoun = getName.StartsWith("You") ? "have" : "has";
                        string reminderOrEvent = activity.Reminder ?
                            string.Format("You have an upcoming reminder - {0} at {1}", label, starting) :
                            string.Format("{0} {3} an upcoming event today - {1} starting {2}", getName, label, starting, pronoun);

                        currentActivity.Add(new Notification
                        {
                            Id = activity.EventID,
                            UserId = activity.UserID,
                            Avatar = avatar,
                            Text = reminderOrEvent,
                            TypeId = NotificationType.PresentAndUpcomingEvents
                        });
                    }
                    else
                    {
                        string pronoun = getName.StartsWith("You") ? "are" : "is";

                        currentActivity.Add(new Notification
                        {
                            Id = activity.EventID,
                            UserId = activity.UserID,
                            Avatar = avatar,
                            Text = string.Format("{0} {3} currently at an event - {1} {2}", getName, label, finishing, pronoun),
                            TypeId = NotificationType.PresentAndUpcomingEvents
                        });
                    }
                }
            }

            return currentActivity;
        }

        public async Task<IEnumerable<Event>> GetEventsAsync(RequestEventDTO request, bool cacheRemove = false)
        {
            string cacheName = $"{cachePrefix}.{nameof(GetAllAsync)}.{Json.Encode(request)}";

            if (cacheRemove)
            {
                cache.Remove(cacheName);
            }

            return await cache.GetAsync(cacheName, async () => await eventRepository.GetAllAsync(request));
        }

        public async Task<IEnumerable<Event>> GetAllAsync(User user, RequestEventDTO request)
        {
            var accessibleCalendars = (await GetAccessibleCalendars(user.UserID)).Select(x => x.Id);

            if (!accessibleCalendars.Any(x => request.CalendarIds.Any(y => x == y)))
            {
                throw new ApplicationException("No permission to view calendar events");
            }

            var events = await GetEventsAsync(request);

            if (user.CronofyReady == CronofyStatus.AuthenticatedRightsSet)
            {
                var deletedEvents = new List<(string EventUid, string CalendarUid)>();
       
                foreach (var extCal in user.ExtCalendarRights.Where(x => x.Read == true))
                {
                    var cronofyEvents = cronofyService.ReadEventsForCalendar(extCal.SyncFromCalendarId).Where(x => x.EventId == null && x.Start.Date.DateTime > DateUtils.UtcDateTime());

                    // check events deleted in Cronofy but not deleted in Calendar App
                    var deletedCronofyEvents = events.Where(x => x.EventUid != null && x.CalendarUid != null && x.CalendarUid == extCal.SyncFromCalendarId && cronofyEvents.All(c => c.EventUid != x.EventUid));
      
                    if (deletedCronofyEvents != null && deletedCronofyEvents.Any())
                    {
                        foreach (var delEvent in deletedCronofyEvents)
                        {
                            deletedEvents.Add((delEvent.EventUid, delEvent.CalendarUid));
                        }
                    }

                    // check events in Cronofy calendar not synced in Calendar App 
                    if (cronofyEvents != null && cronofyEvents.Any())
                    {
                        foreach (var e in cronofyEvents)
                        {
                            var extCalendar = user.ExtCalendarRights.FirstOrDefault(x => x.SyncFromCalendarId == e.CalendarId);
        
                            // insert to Appology Calendar
                            if (extCalendar != null)
                            {
                                Event eventToUpdate = null;
                                DateTime utcStartDate = e.Start.HasTime ? e.Start.DateTimeOffset.UtcDateTime : e.Start.Date.DateTime.Date;

                                bool eventUExistsAndUpdated = await eventRepository.EventUExists(e.EventUid, e.CalendarId) && e.Updated > e.Created;
                                bool eventExistsAtStartTime = await EventExistsAtStartTime(utcStartDate, extCalendar.SyncToCalendarId);

                                if (eventUExistsAndUpdated || !eventExistsAtStartTime)
                                {
                                    if (eventUExistsAndUpdated)
                                    {
                                        var getEvent = await eventRepository.GetUEvent(e.EventUid, e.CalendarId);

                                        if (getEvent != null)
                                        {
                                            var modificationDate = getEvent.Modified ?? getEvent.Created;
                                            if (e.Updated > modificationDate)
                                            {
                                                eventToUpdate = getEvent;
                                            }
                                        }
                                    }

                                    if (eventToUpdate != null || !eventExistsAtStartTime)
                                    {
                                        DateTime startDate = e.Start.HasTime ? DateUtils.FromTimeZoneToUtc(e.Start.DateTimeOffset.DateTime) : DateUtils.FromTimeZoneToUtc(e.Start.Date.DateTime.Date);
                                        DateTime endDate = e.Start.HasTime ? DateUtils.FromTimeZoneToUtc(e.End.DateTimeOffset.DateTime) : DateUtils.FromTimeZoneToUtc(e.End.Date.DateTime.Date);

                                        var findTag = (await userService.GetUserTags(user.UserID))
                                            .FirstOrDefault(t => Utils.Contains(t.Name, e.Summary ?? "(No title)", StringComparison.OrdinalIgnoreCase));

                                        string calendarName = cronofyService.GetCalendars().FirstOrDefault(x => x.CalendarId == e.CalendarId)?.Profile.ProviderName ?? "Unknown";

                                        var model = new Event
                                        {
                                            UserID = user.UserID,
                                            TagID = findTag?.Id ?? Guid.Empty,
                                            Description = string.Format($"{(findTag != null ? "" : "{1}")} {{0}}", e.Description, e.Summary),
                                            StartDate = startDate,
                                            EndDate = endDate,
                                            EventUid = e.EventUid,
                                            CalendarId = extCalendar.SyncToCalendarId,
                                            CalendarUid = e.CalendarId,
                                            IsFullDay = !e.Start.HasTime,
                                            Provider = Utils.UppercaseFirst(calendarName)
                                        };

                                        if (eventToUpdate != null)
                                        {
                                            model.EventID = eventToUpdate.EventID;
                                            await eventRepository.InsertOrUpdateAsync(model);
                                            RemoveAll();

                                        }
                                        else if (!eventExistsAtStartTime)
                                        {
                                            model.EventID = Guid.NewGuid();
                                        }

                                        await eventRepository.InsertOrUpdateAsync(model);
                                        RemoveAll();
                                    }
                                }
                            }
                        }
                    }
                }

                if (deletedEvents.Any())
                {
                    foreach (var delEvent in deletedEvents)
                    {
                        await eventRepository.DeleteExtAsync(delEvent.EventUid, delEvent.CalendarUid);
                    }
                }
            }

            return events;
        }

        public async Task SaveCronofyEvent(Event e, IEnumerable<ExtCalendarRights> Rights)
        {
            string subject;
            string color;

            if (e.TagID != null && e.TagID != Guid.Empty)
            {
                var tag = await userService.GetUserTagAysnc(e.TagID.Value);
                subject = tag.Name;
                color = tag.ThemeColor;
            }
            else
            {
                subject = e.Description;
                color = "#eee";
            }

            DateTime endDate = e.EndDate ?? e.StartDate.AddDays(1);

            int[] reminders = null;
            if (!string.IsNullOrEmpty(e.Alarm))
            {
                reminders = e.Alarm.Split(',').Select(x => int.Parse(x)).ToArray();
            }
 
            foreach (var extCal in Rights)
            {
                if (extCal.Save)
                {
                    cronofyService.UpsertEvent(e.EventID.ToString(), extCal.SyncFromCalendarId, subject, e.Description, e.StartDate, endDate, color, reminders);
                }
            }

            RemoveAll();
        }

        public async Task<bool> SaveEvent(Event dto)
        {
            //if (dto.EventID != Guid.Empty)
            //{
            //    var getEvent = await GetAsync(dto.EventID);
            //    dto.CalendarUid = getEvent.CalendarUid;
            //    dto.EventUid = getEvent.EventUid;
            //    dto.Provider = getEvent.Provider;
            //}
            //else
            //{
            //    dto.EventID = Guid.NewGuid();
            //}

            RemoveAll();
            return (await eventRepository.InsertOrUpdateAsync(dto)).Status;
        }

        public async Task<Event> SaveGetEvent(Event dto)
        {
            RemoveAll();
            return (await eventRepository.InsertOrUpdateAsync(dto)).e;
        }

        public void DeleteCronofyEvent(string syncFromCalendarId, Guid eventId)
        {
            cronofyService.DeleteEvent(syncFromCalendarId, eventId.ToString());
        }

        public async Task<bool> DeleteEvent(Guid eventId, string eventUid = null)
        {
            RemoveAll();
            return await eventRepository.DeleteAsync(eventId);
        }


        public async Task<bool> EventExistsAtStartTime(DateTime startDate, int calendarId)
        {
            return await eventRepository.EventExistsAtStartTime(startDate, calendarId);
        }

        public async Task<bool> EventExistsInCalendar(int calendarId)
        {
            return await eventRepository.EventExistsInCalendar(calendarId);
        }

        private void RemoveAll()
        {
            cache.RemoveAll(cachePrefix);
        }
    }
}