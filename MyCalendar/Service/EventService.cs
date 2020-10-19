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
        Task<IEnumerable<Event>> GetAllAsync(Guid? userId = null, Guid? viewing = null, DateFilter filter = null);
        Task<bool> SaveEvent(EventVM e);
        Task<bool> SaveEvents(IList<Model.EventDTO> dto, bool cronofy);
        Task<bool> DeleteEvent(Guid eventId);
        Task<IEnumerable<Types>> GetTypes();
        Task<Types> GetTypeAsync(int Id);
        Task<IEnumerable<Event>> GetCurrentActivityAsync();
        Task<IList<HoursWorkedInTag>> HoursSpentInTag(Guid userID, DateFilter dateFilter);
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

        public async Task<IEnumerable<Event>> GetAllAsync(Guid? userId = null, Guid? viewing = null, DateFilter filter = null)
        {
            var events = await eventRepository.GetAllAsync(filter);

            if (userId.HasValue)
            {
                if (viewing.HasValue)
                {
                    if (userId == viewing)
                    {
                        events = events.Where(x => x.UserID == userId || x.Privacy == TagPrivacy.Shared);
                    }

                    else
                    {
                        events = events.Where(x => (x.UserID == viewing && x.Privacy != TagPrivacy.Private) || x.Privacy == TagPrivacy.Shared);
                    }
                }
                //combined
                else
                {
                    events = events.Where(x => (x.UserID != userId && x.Privacy != TagPrivacy.Private) || x.UserID == userId || x.Privacy == TagPrivacy.Shared);
                }
            }

            return events;
        }

        private async Task SaveCronofyEvent(Guid eventID, DateTime start, string desc, Guid? tagId, DateTime? end)
        {
            var user = await userService.GetUser();

            if (user.CronofyReady)
            {
                string subject;
                string color;

                if (tagId != Guid.Empty)
                {
                    var tag = await userService.GetUserTagAysnc(tagId.Value);
                    subject = tag.Name;
                    color = tag.ThemeColor;
                }
                else
                {
                    subject = desc;
                    color = "#eee";
                }


                DateTime endDate = end ?? start.AddDays(1);
                cronofyService.UpsertEvent(eventID.ToString(), user.DefaultCalendar, subject, desc, start, endDate, color);
            }
        }

        public async Task<bool> SaveEvent(EventVM e)
        {
            var dto = DTOs.EventDTO.MapFrom(e);
            var daysBetweenDays = e.End.HasValue ? (e.End.Value.Date - e.Start.Date).Days : 0;

            bool status;
            if (string.IsNullOrEmpty(e.SplitDates) || daysBetweenDays == 0 || e.IsFullDay)
            {
                status = await eventRepository.InsertOrUpdateAsync(dto);

                if (e.Cronofy)
                {
                    e.EventID = e.EventID != Guid.Empty ? e.EventID : Guid.NewGuid();
                    await SaveCronofyEvent(e.EventID, e.Start,e.Description, e.TagID, e.End);
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
                        IsFullDay = dto.IsFullDay,
                        TagID = dto.TagID,
                        Tentative = dto.Tentative,
                        UserID = dto.UserID
                    });
                }

                status = await SaveEvents(events.ToList(), e.Cronofy);
            }

            return status;
        }

        public async Task<bool> SaveEvents(IList<Model.EventDTO> dto, bool cronofy)
        {
            if (cronofy)
            {
                foreach (var e in dto)
                {
                    e.EventID = e.EventID != Guid.Empty ? e.EventID : Guid.NewGuid();
                    await SaveCronofyEvent(e.EventID, e.StartDate, e.Description, e.TagID, e.EndDate);
                }
            }

            return await eventRepository.MultiInsertAsync(dto);
        }

        public async Task<IList<HoursWorkedInTag>> HoursSpentInTag(Guid userID, DateFilter dateFilter)
        {
            var users = await userService.GetUsers();
            var events = (await GetAllAsync(userId: null, viewing: null, filter: dateFilter))
                .Where(x => x.UserID == userID || x.Privacy == TagPrivacy.Shared)
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
                                text += $" earning approx £{hoursFromMinutes * 13}";
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

            if (user.CronofyReady)
            {
                cronofyService.DeleteEvent(user.DefaultCalendar, eventId.ToString());
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
    }
}
