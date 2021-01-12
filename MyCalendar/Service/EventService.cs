using MyCalendar.DTOs;
using MyCalendar.Enums;
using MyCalendar.Helpers;
using MyCalendar.Model;
using MyCalendar.Repository;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MyCalendar.Service
{
    public interface IEventService
    {
        Task<Event> GetAsync(Guid eventId);
        Task<IEnumerable<Event>> GetAllAsync(User user, RequestEventDTO request);
        Task<bool> SaveEvent(Event dto);
        Task<Event> SaveGetEvent(Event dto);
        Task<bool> DeleteEvent(Guid eventId, string eventUid = null);
        Task<IEnumerable<Event>> GetCurrentActivityAsync();
        Task<IList<HoursWorkedInTag>> HoursSpentInTag(User user, DateFilter dateFilter);
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
        private readonly IEventRepository eventRepository;
        private readonly ICronofyService cronofyService;
        private readonly IUserService userService;
        private readonly ITypeService typeService;

        public EventService(
            IEventRepository eventRepository, 
            ICronofyService cronofyService, 
            ITypeService typeService,
            IUserService userService)
        {
            this.eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
            this.cronofyService = cronofyService ?? throw new ArgumentNullException(nameof(Cronofy));
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.typeService = typeService ?? throw new ArgumentNullException(nameof(typeService));
        }

        public async Task<Event> GetAsync(Guid eventId)
        {
            return await eventRepository.GetAsync(eventId);
        }

        public async Task<IEnumerable<Event>> GetCurrentActivityAsync()
        {
            return (await eventRepository.GetCurrentActivityAsync())
                .Where(x => (Utils.DateTime() >= Utils.FromUtcToTimeZone(x.StartDate.AddHours(-4)) && x.EndDate.HasValue && Utils.DateTime() < Utils.FromUtcToTimeZone(x.EndDate.Value)) ||
                        (!x.EndDate.HasValue && Utils.FromUtcToTimeZone(x.StartDate).Date == Utils.DateTime().Date));
        }

        public async Task<IEnumerable<Types>> GetAccessibleCalendars(Guid userId)
        {
            return (await typeService.GetUserTypesAsync(userId)).Where(x => x.GroupId == TypeGroup.Calendars);
        }

        public async Task<IEnumerable<Types>> GetUserCalendars(Guid userId)
        {
            return (await typeService.GetAllByUserIdAsync(userId)).Where(x => x.GroupId == TypeGroup.Calendars);
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
                            var inviteeName = (await userService.GetByUserIDAsync(invitee)).Name;
                            inviteeList.Add(inviteeName);
                        }

                        getName += $"You, {string.Join(", ", string.Join(", ", inviteeList))}";
                    }
                    else
                    {
                        getName = (userId == activity.UserID ? "You" : user.Name);
                    }

                    string label = activity.Subject ?? activity.Description;
                    string finishing = (activity.EndDate.HasValue ? "finishing " + Utils.FromUtcToTimeZone(activity.EndDate.Value).ToString("HH:mm") : "for the day");
                    string starting = Utils.FromUtcToTimeZone(activity.StartDate).ToString("HH:mm");
                    string avatar = Utils.AvatarSrc(user.UserID, user.Avatar, user.Name);

                    if (Utils.DateTime() >= Utils.FromUtcToTimeZone(activity.StartDate.AddHours(-4)) && Utils.DateTime() < Utils.FromUtcToTimeZone(activity.StartDate))
                    {
                        string pronoun = getName.StartsWith("You") ? "have" : "has";

                        currentActivity.Add(new Notification
                        {
                            Avatar = avatar,
                            Text = string.Format("{0} {3} an upcoming event today - {1} starting {2}", getName, label, starting, pronoun),
                            Feature = Features.Calendar
                        });
                    }
                    else
                    {
                        string pronoun = getName.StartsWith("You") ? "are" : "is";

                        currentActivity.Add(new Notification
                        {
                            Avatar = avatar,
                            Text = string.Format("{0} {3} currently at an event - {1} {2}", getName, label, finishing, pronoun),
                            Feature = Features.Calendar
                        });
                    }
                }
            }

            return currentActivity;
        }

        public async Task<IEnumerable<Event>> GetAllAsync(User user, RequestEventDTO request)
        {
            var accessibleCalendars = (await GetAccessibleCalendars(user.UserID)).Select(x => x.Id);

            if (!accessibleCalendars.Any(x => request.CalendarIds.Any(y => x == y)))
            {
                throw new ApplicationException("No permission to view calendar events");
            }

            var events = await eventRepository.GetAllAsync(request);

            if (user.CronofyReady == CronofyStatus.AuthenticatedRightsSet)
            {
                var deletedEvents = new List<(string EventUid, string CalendarUid)>();
       
                foreach (var extCal in user.ExtCalendarRights.Where(x => x.Read == true))
                {
                    var cronofyEvents = cronofyService.ReadEventsForCalendar(extCal.SyncFromCalendarId).Where(x => x.EventId == null && x.Start.Date.DateTime > Utils.UtcDateTime());

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
                                        DateTime startDate = e.Start.HasTime ? Utils.FromTimeZoneToUtc(e.Start.DateTimeOffset.DateTime) : Utils.FromTimeZoneToUtc(e.Start.Date.DateTime.Date);
                                        DateTime endDate = e.Start.HasTime ? Utils.FromTimeZoneToUtc(e.End.DateTimeOffset.DateTime) : Utils.FromTimeZoneToUtc(e.End.Date.DateTime.Date);

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

                                        }
                                        else if (!eventExistsAtStartTime)
                                        {
                                            model.EventID = Guid.NewGuid();
                                        }

                                        await eventRepository.InsertOrUpdateAsync(model);
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

            return (await eventRepository.InsertOrUpdateAsync(dto)).Status;
        }

        public async Task<Event> SaveGetEvent(Event dto)
        {
            return (await eventRepository.InsertOrUpdateAsync(dto)).e;
        }

        public async Task<IList<HoursWorkedInTag>> HoursSpentInTag(User user, DateFilter dateFilter)
        {
            var userCalendarIds = (await userService.UserCalendars(user.UserID)).Select(x => x.Id).ToArray();

            var eventRequest = new RequestEventDTO
            {
                CalendarIds = userCalendarIds,
                DateFilter = dateFilter
            };

            var events = (await GetAllAsync(user, eventRequest))
                .Where(x => x.UserID == user.UserID || x.InviteeIdsList.Contains(user.UserID))
                .GroupBy(x => x.TagID);

            var hoursWorkedInTag = new List<HoursWorkedInTag>();

            if (events != null && events.Any())
            {
                foreach (var e in events)
                {
                    if (e.Key.HasValue && e.Key != Guid.Empty)
                    {
                        var tag = await userService.GetUserTagAysnc(e.Key.Value);
                        string userName = "You";
                        bool multiUser = false;

                        if (tag.InviteeIdsList.Any())
                        {
                            var inviteeList = new List<string>();
                            foreach (var invitee in tag.InviteeIdsList)
                            {
                                var id = invitee == user.UserID ? tag.UserID : invitee;
                                var inviteeName = (await userService.GetByUserIDAsync(id)).Name;
                                inviteeList.Add(inviteeName);
                            }

                            userName += ", " + string.Join(", ", inviteeList);
                            multiUser = true;
                        }

                        double minutesWorked = e.Sum(x => x.EndDate.HasValue ? Utils.MinutesBetweenDates(x.EndDate.Value, x.StartDate) : 1440);

                        if (minutesWorked > 0)
                        {
                            int hoursFromMinutes = Utils.GetHoursFromMinutes(minutesWorked);
                            int calculateHours = hoursFromMinutes >= 672 ? hoursFromMinutes / 4 : 0;
                            string averaged = calculateHours >= 1 ? $" averaging {calculateHours} hour{(calculateHours > 1 ? "s" : "")} a week" : "";
                            string text;

                            if (dateFilter.Frequency == DateFrequency.Upcoming)
                            {
                                string multipleEvents = e.Count() > 1 ? "have upcoming events totalling" : "have an upcoming event for";
                                text = string.Format($"{userName} {multipleEvents} {Utils.HoursDurationFromMinutes(minutesWorked)} with {tag.Name}");
                            }
                            else
                            {
                                text = string.Format($"{userName} spent {Utils.HoursDurationFromMinutes(minutesWorked)} {averaged} with {tag.Name}");
                            }

                            if (tag.Name == "Flex")
                            {
                                text += $" earning approx £{hoursFromMinutes * 14 * 1.15}";
                            }

                            hoursWorkedInTag.Add(new HoursWorkedInTag
                            {
                                Text = text,
                                MultiUsers = multiUser,
                                Color = tag.ThemeColor,
                                ContrastColor = Utils.ContrastColor(tag.ThemeColor),
                                TypeName = tag.TypeName,
                                ActivityTag = multiUser ? "fa-user-friends" : "fa-tag"
                            });
                        }
                    }
                }
            }

            return hoursWorkedInTag;
        }

        public void DeleteCronofyEvent(string syncFromCalendarId, Guid eventId)
        {
            cronofyService.DeleteEvent(syncFromCalendarId, eventId.ToString());
        }

        public async Task<bool> DeleteEvent(Guid eventId, string eventUid = null)
        {
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
    }
}
