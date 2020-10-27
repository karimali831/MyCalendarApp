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

namespace MyCalendar.Service
{
    public interface IEventService
    {
        Task<Event> GetAsync(Guid eventId);
        Task<IEnumerable<Event>> GetAllAsync(User user, Guid? viewing = null, DateFilter filter = null);
        Task<bool> SaveEvent(EventVM e);
        Task<bool> SaveEvents(IList<Model.EventDTO> dto);
        Task<bool> DeleteEvent(Guid eventId);
        Task<IEnumerable<Types>> GetTypes();
        Task<Types> GetTypeAsync(int Id);
        Task<IEnumerable<Event>> GetCurrentActivityAsync();
        Task<IList<HoursWorkedInTag>> HoursSpentInTag(User user, DateFilter dateFilter);
        Task<bool> EventUExists(string eventUId, string calendarUid);
    }

    public class EventService : IEventService
    {
        private readonly IEventRepository eventRepository;
        private readonly ITypeService typeService;
        private readonly ICronofyService cronofyService;
        private readonly IUserService userService;

        public EventService(
            IEventRepository eventRepository, 
            ITypeService typeService, 
            ICronofyService cronofyService, 
            IUserService userService)
        {
            this.eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
            this.typeService = typeService ?? throw new ArgumentNullException(nameof(typeService));
            this.cronofyService = cronofyService ?? throw new ArgumentNullException(nameof(Cronofy));
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
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

        public async Task<IEnumerable<Event>> GetAllAsync(User user, Guid? viewing = null, DateFilter filter = null)
        {
            var events = await eventRepository.GetAllAsync(filter);

            if (viewing.HasValue)
            {
                if (user.UserID == viewing)
                {
                    events = events.Where(x => x.UserID == user.UserID || x.Privacy == TagPrivacy.Shared);
                }

                else
                {
                    events = events.Where(x => (x.UserID == viewing && x.Privacy != TagPrivacy.Private) || x.Privacy == TagPrivacy.Shared);
                }
            }
            //combined
            else
            {
                events = events.Where(x => (x.UserID != user.UserID && x.Privacy != TagPrivacy.Private) || x.UserID == user.UserID || x.Privacy == TagPrivacy.Shared);
            }

            if (user.CronofyReady == CronofyStatus.AuthenticatedRightsSet)
            {
                var unsycnedEvents = new List<Model.EventDTO>();
                var deletedEvents = new List<(string EventUid, string CalendarUid)>();
       
                foreach (var extCal in user.ExtCalendarRights.Where(x => x.Read == true))
                {
                    var cronofyEvents = cronofyService.ReadEventsForCalendar(extCal.Id).Where(x => x.EventId == null);

                    // check events deleted in Cronofy but not deleted in Calendar App
                    var deletedCronofyEvents = events.Where(x => x.EventUid != null && x.CalendarUid != null && x.CalendarUid == extCal.Id && cronofyEvents.All(c => c.EventUid != x.EventUid));
      
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
                            if (!await EventUExists(e.EventUid, e.CalendarId))
                            {
                                var findTag = (await userService.GetUserTags(user.UserID))
                                    .FirstOrDefault(t => Utils.Contains(t.Name, e.Summary, StringComparison.OrdinalIgnoreCase));
       
                                string calendarName = cronofyService.GetCalendars().FirstOrDefault(x => x.CalendarId == e.CalendarId)?.Profile.ProviderName ?? "Unknown";

                                if (e.Start.HasTime)
                                {
                                    unsycnedEvents.Add(new Model.EventDTO
                                    {
                                        EventID = Guid.NewGuid(),
                                        UserID = user.UserID,
                                        TagID = findTag?.Id ?? Guid.Empty,
                                        Description = string.Format($"{(findTag != null ? "" : "{2}: ")} {{1}} (Sycned from {{0}} Calendar)", Utils.UppercaseFirst(calendarName), e.Description, e.Summary),
                                        StartDate = e.Start.DateTimeOffset.DateTime,
                                        EndDate = e.End.DateTimeOffset.DateTime,
                                        EventUid = e.EventUid,
                                        CalendarUid = e.CalendarId
                                    });
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

                if (dto.TagID != Guid.Empty)
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

                foreach (var extCal in user.ExtCalendarRights)
                {
                    if (extCal.Save)
                    {
                        cronofyService.UpsertEvent(dto.EventID.ToString(), extCal.Id, subject, dto.Description, dto.StartDate, endDate, color);
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
                dto.EventID = dto.EventID != Guid.Empty ? dto.EventID : Guid.NewGuid();

                status = await eventRepository.InsertOrUpdateAsync(dto);
                await SaveCronofyEvent(dto);
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
                        IsFullDay = dto.IsFullDay,
                        TagID = dto.TagID,
                        Tentative = dto.Tentative,
                        UserID = dto.UserID
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
                await SaveCronofyEvent(e);
            }
            
            return await eventRepository.MultiInsertAsync(dto);
        }

        public async Task<IList<HoursWorkedInTag>> HoursSpentInTag(User user, DateFilter dateFilter)
        {
            var users = await userService.GetUsers(user.UserID);
            var events = (await GetAllAsync(user, viewing: null, filter: dateFilter))
                .Where(x => x.UserID == user.UserID || x.Privacy == TagPrivacy.Shared)
                .GroupBy(x => x.TagID);

            var hoursWorkedInTag = new List<HoursWorkedInTag>();

            if (events != null && events.Any())
            {
                foreach (var e in events)
                {
                    if (e.Key != Guid.Empty)
                    {
                        var tag = await userService.GetUserTagAysnc(e.Key);
                        var type = await GetTypeAsync(tag.TypeID);
                        string userName = "You";
                        bool multiUser = false;

                        if (tag.Privacy == TagPrivacy.Shared)
                        {
                            userName += ", " + string.Join(", ", users.Select(x => x.Name));
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
                                TypeName = type.Name
                            });
                        }
                    }
                }
            }

            return hoursWorkedInTag;
        }

        public async Task<bool> DeleteEvent(Guid eventId)
        {
            // delete from cronofy
            var user = await userService.GetUser();

            if (user.CronofyReady == CronofyStatus.AuthenticatedRightsSet)
            {
                foreach (var extCal in user.ExtCalendarRights)
                {
                    if (extCal.Delete)
                    {
                        cronofyService.DeleteEvent(extCal.Id, eventId.ToString());
                    }
                }
            }

            return await eventRepository.DeleteAsync(eventId);
        }

        public async Task<IEnumerable<Types>> GetTypes()
        {
            return await typeService.GetAllAsync();
        }

        public async Task<Types> GetTypeAsync(int Id)
        {
            return await typeService.GetAsync(Id);
        }

        public async Task<bool> EventUExists(string eventUId, string calendarUid)
        {
            return await eventRepository.EventUExists(eventUId, calendarUid);
        }
    }
}
