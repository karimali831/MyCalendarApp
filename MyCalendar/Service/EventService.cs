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
        Task<IEnumerable<Event>> GetAllAsync(User user, int[] calendarIds, DateFilter filter = null);
        Task<bool> SaveEvent(EventVM e);
        Task<bool> SaveEvents(IList<Model.EventDTO> dto);
        Task<bool> DeleteEvent(Guid eventId, string eventUid = null);
        Task<IEnumerable<Event>> GetCurrentActivityAsync();
        Task<IList<HoursWorkedInTag>> HoursSpentInTag(User user, DateFilter dateFilter);
        Task<bool> EventExistsInCalendar(int calendarId);
        Task<IEnumerable<Types>> GetAccessibleCalendars(Guid userId);
        Task<IEnumerable<Types>> GetUserCalendars(Guid userId);
        Task<bool> EventExistsAtStartTime(DateTime startDate, int calendarId);
        Task<string> GetLastStoredAlarm(Guid tagId);
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

        public async Task<IEnumerable<Event>> GetAllAsync(User user, int[] calendarIds, DateFilter filter = null)
        {
            var accessibleCalendars = (await GetAccessibleCalendars(user.UserID)).Select(x => x.Id);

            if (!accessibleCalendars.Any(x => calendarIds.Any(y => x == y)))
            {
                throw new ApplicationException("No permission to view calendar events");
            }

            var events = await eventRepository.GetAllAsync(calendarIds, filter);

            if (user.CronofyReady == CronofyStatus.AuthenticatedRightsSet)
            {
                var unsycnedEvents = new List<Model.EventDTO>();
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

                                        var model = new Model.EventDTO
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
                                            unsycnedEvents.Add(model);
                                        }
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

                if (unsycnedEvents.Any())
                {
                    await SaveEvents(unsycnedEvents);
                }
            }

            return events;
        }

        private async Task SaveCronofyEvent(Model.EventDTO dto)
        {
            var user = await userService.GetUser();

            if (user.CronofyReady == CronofyStatus.AuthenticatedRightsSet)
            {
                string subject;
                string color;

                if (dto.TagID != null && dto.TagID != Guid.Empty)
                {
                    var tag = await userService.GetUserTagAysnc(dto.TagID.Value);
                    subject = tag.Name;
                    color = tag.ThemeColor;
                }
                else
                {
                    subject = dto.Description;
                    color = "#eee";
                }

                DateTime endDate = dto.EndDate ?? dto.StartDate.AddDays(1);

                int[] reminders = null;
                if (!string.IsNullOrEmpty(dto.Alarm))
                {
                    reminders = dto.Alarm.Split(',').Select(x => int.Parse(x)).ToArray();
                }

 
                foreach (var extCal in user.ExtCalendarRights)
                {
                    if (extCal.Save)
                    {
                        cronofyService.UpsertEvent(dto.EventID.ToString(), extCal.SyncFromCalendarId, subject, dto.Description, dto.StartDate, endDate, color, reminders);
                    }
                }
            }
        }


        public async Task<bool> SaveEvent(EventVM e)
        {
            var dto = DTOs.EventDTO.MapFrom(e);
            var daysBetweenDays = e.End.HasValue ? (e.End.Value.Date - e.Start.Date).Days : 0;
       
            bool status;
            if (string.IsNullOrEmpty(e.SplitDates) || daysBetweenDays == 0 || e.IsFullDay)
            {
                if (dto.EventID != Guid.Empty)
                {
                    var getEvent = await GetAsync(e.EventID);
                    dto.CalendarUid = getEvent.CalendarUid;
                    dto.EventUid = getEvent.EventUid;
                    dto.Provider = getEvent.Provider;
                }
                else
                {
                    dto.EventID = Guid.NewGuid();
                }

                status = await eventRepository.InsertOrUpdateAsync(dto);

                // do not amend Cronofy events created in third party calendar
                if (dto.EventUid == null)
                {
                    await SaveCronofyEvent(dto);
                }
            }
            else
            {
                var events = new List<Model.EventDTO>();

                for (var date = e.Start; date <= e.End; date = date.AddDays(1))
                {
                    events.Add(new Model.EventDTO
                    {
                        StartDate = date,
                        EndDate = new DateTime(date.Year, date.Month, date.Day, e.End.Value.Hour, e.End.Value.Minute, 0),
                        Description = dto.Description,
                        EventID = dto.EventID,
                        CalendarId = dto.CalendarId,
                        IsFullDay = dto.IsFullDay,
                        TagID = dto.TagID,
                        Tentative = dto.Tentative,
                        UserID = dto.UserID,
                        Alarm = dto.Alarm
                    });
                }

                status = await SaveEvents(events.ToList());
            }

            return status;
        }

        public async Task<bool> SaveEvents(IList<Model.EventDTO> dto)
        {
            foreach (var e in dto.Where(x => x.EventUid == null))
            {
                e.EventID = e.EventID != Guid.Empty ? e.EventID : Guid.NewGuid();
                e.StartDate = Utils.FromTimeZoneToUtc(e.StartDate);
                e.EndDate = e.EndDate.HasValue? Utils.FromTimeZoneToUtc(e.EndDate.Value) : (DateTime?)null;

                await SaveCronofyEvent(e);
            }
            
            return await eventRepository.MultiInsertAsync(dto);
        }

        public async Task<IList<HoursWorkedInTag>> HoursSpentInTag(User user, DateFilter dateFilter)
        {
            var userCalendarIds = (await userService.UserCalendars(user.UserID)).Select(x => x.Id).ToArray();

            var events = (await GetAllAsync(user, userCalendarIds, filter: dateFilter))
                .Where(x => x.UserID == user.UserID || x.InviteeIdsList.Contains(user.UserID))
                .GroupBy(x => x.TagID);

            var hoursWorkedInTag = new List<HoursWorkedInTag>();

            if (events != null && events.Any())
            {
                foreach (var e in events)
                {
                    if (e.Key != Guid.Empty)
                    {
                        var tag = await userService.GetUserTagAysnc(e.Key);
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

        public async Task<bool> DeleteEvent(Guid eventId, string eventUid = null)
        {
            // delete from Cronofy if event was created from Appology Calendar 
            var user = await userService.GetUser();

            if (user.CronofyReady == CronofyStatus.AuthenticatedRightsSet && eventUid == null)
            {
                foreach (var extCal in user.ExtCalendarRights)
                {
                    if (extCal.Delete)
                    {
                        cronofyService.DeleteEvent(extCal.SyncFromCalendarId, eventId.ToString());
                    }
                }
            }

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
