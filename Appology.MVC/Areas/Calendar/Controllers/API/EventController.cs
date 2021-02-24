using Appology.Helpers;
using Appology.Service;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using Appology.Model;
using Appology.MiCalendar.Service;
using Appology.MiCalendar.Enums;
using Appology.MiCalendar.DTOs;
using Appology.MiCalendar.Helpers;
using Appology.MiCalendar.Model;
using Appology.Controllers.Api;
using System.Configuration;

namespace Appology.Areas.MiCalendar.Controllers.API
{
    [RoutePrefix("api/calendar")]
    [CamelCaseControllerConfig]
    public class EventController : ApiController
    {
        private readonly IEventService eventService;
        private readonly IUserService userService;
        private readonly string rootUrl = ConfigurationManager.AppSettings["RootUrl"];

        public EventController(IEventService eventService, IUserService userService)
        {
            this.eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        private async Task<User> GetUser()
        {
            bool isLocal = this.rootUrl == "http://localhost:53822";
            return await userService.GetUser(isLocal ? "karimali831@googlemail.com" : null);
        }

        [Route("usertags")]
        [HttpGet]
        public async Task<HttpResponseMessage> UserTags()
        {
            var user = await GetUser();
            var userTags = await userService.GetUserTags(user.UserID);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                userTags,
                cronofyReady =
                    (user.CronofyReady == CronofyStatus.AuthenticatedRightsSet)

            });
        }

        [Route("alarminfo/{Id}")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetAlarmInfo(string Id)
        {
            if (Guid.TryParse(Id, out Guid tagId))
            {
                var alarm = await eventService.GetLastStoredAlarm(tagId);
                return Request.CreateResponse(HttpStatusCode.OK, alarm ?? "");
            }

            return Request.CreateResponse(HttpStatusCode.BadRequest, false);
        }

        [Route("delete/{Id}")]
        [HttpGet]
        public async Task<HttpResponseMessage> DeleteEvent(string Id)
        {
            if (Guid.TryParse(Id, out Guid eventId))
            {
                var user = await GetUser();
                var e = await eventService.GetAsync(eventId);

                if (user.UserID != e.UserID || user == null || e == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, false);
                }

                // delete from Cronofy if event was created from Appology Calendar 
                if (user.CronofyReady == CronofyStatus.AuthenticatedRightsSet && e.EventUid == null)
                {
                    foreach (var extCal in user.ExtCalendarRights)
                    {
                        if (extCal.Delete)
                        {
                            eventService.DeleteCronofyEvent(extCal.SyncFromCalendarId, eventId);
                        }
                    }
                }

                var status = await eventService.DeleteEvent(eventId, e.EventUid);
                return Request.CreateResponse(HttpStatusCode.OK, status);

            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, false);
            }
        }

        [Route("save/{multiEvents}")]
        [HttpPost]
        public async Task<HttpResponseMessage> SaveEvent(EventDTO dto, bool multiEvents)
        {
            var e = EventDTO.MapFrom(dto);
            var user = await GetUser();

            if (user == null || dto.CalendarId == 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, false);
            }

            if (dto.Id != null && dto.Id != Guid.Empty && user.UserID != dto.UserID)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, false);
            }

            if (user.CronofyReady == CronofyStatus.AuthenticatedRightsSet)
            {
                // do not amend Cronofy events created in third party calendar
                if (dto.EventUid == null)
                {
                    await eventService.SaveCronofyEvent(e, user.ExtCalendarRights);
                }
            }

            e.UserID = user.UserID;
            var daysBetweenDays = e.EndDate.HasValue ? (e.EndDate.Value.Date - e.StartDate.Date).Days : 0;

            var addedEvents = new List<Event>();

            if (multiEvents && daysBetweenDays != 0)
            {

                for (var date = e.StartDate; date <= e.EndDate; date = date.AddDays(1))
                {
                    var newEvent = new Event
                    {
                        EventID = e.EventID,
                        StartDate = date,
                        EndDate = new DateTime(date.Year, date.Month, date.Day, e.EndDate.Value.Hour, e.EndDate.Value.Minute, 0),
                        Description = e.Description,
                        IsFullDay = false,
                        CalendarId = e.CalendarId,
                        Reminder = false,
                        TagID = e.TagID,
                        Tentative = e.Tentative,
                        EventUid = e.EventUid,
                        UserID = e.UserID,
                        Alarm = e.Alarm
                    };

                    addedEvents.Add(await eventService.SaveGetEvent(newEvent));
                }
            }
            else
            {
                addedEvents.Add(await eventService.SaveGetEvent(e));
            }

            if (addedEvents.Any())
            {
                var getEvents = MapDTO(addedEvents, user.UserID).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, getEvents);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, false);
            }
            
        }

        private IEnumerable<object> MapDTO(IEnumerable<Event> events, Guid userId)
        {
            return events.Select(x => new
            {
                // standard props
                id = x.EventID,
                title = x.Reminder ? x.Description : x.Subject ?? x.Description,
                start = x.StartDate,
                end = x.EndDate,
                startStr = x.StartDate.ToString("s", CultureInfo.InvariantCulture),
                endStr = x.EndDate.HasValue ? x.EndDate.Value.ToString("s", CultureInfo.InvariantCulture) : null,
                allDay = x.IsFullDay,
                url = "",
                classNames = "",
                editable = false, 
                backgroundColor = x.ThemeColor ?? "lightslategrey",
                // non standard props
                calendarId = x.CalendarId,
                userId = x.UserID,
                tagId = x.TagID,
                description = x.Description,
                tentative = x.Tentative,
                duration = x.EndDate.HasValue ? DateUtils.Duration(x.EndDate.Value, x.StartDate) : string.Empty,
                eventUid = x.EventUid,
                alarm = x.Alarm,
                provider = x.Provider,
                reminder = x.Reminder,
                avatar = CalendarUtils.AvatarSrc(x.UserID, x.Avatar, x.Name)
            })
            .Where(x => !x.reminder || (x.reminder && userId == x.userId));
        }

        [Route("events")]
        [HttpPost]
        public async Task<HttpResponseMessage> Get(RequestEventDTO request)
        {
            var user = await GetUser();
            var userCalendars = await userService.UserCalendars(user.UserID);
   
            // user has set calendar seleciton
            if (user.SelectedCalendarsList != null && user.SelectedCalendarsList.Any())
            {
                request.CalendarIds = request.CalendarIds.Length > 0 ? request.CalendarIds : userCalendars
                    .Where(x => user.SelectedCalendarsList.Contains(x.Id))
                    .Select(x => x.Id)
                    .ToArray();
            }
            // user not set calendar selection - get default
            else
            {
                request.CalendarIds = request.CalendarIds.Length > 0 ? request.CalendarIds : new int[] { userCalendars
                    .Where(x => user.UserID == x.UserCreatedId)
                    .Select(x => x.Id)
                    .FirstOrDefault()
                };
            }

            var events = await eventService.GetAllAsync(user, request);

            return Request.CreateResponse(HttpStatusCode.OK, new {
                userId = user.UserID,
                events = MapDTO(events, user.UserID), 
                userCalendars = userCalendars.Select(x => new
                {
                    id = x.Id,
                    name = x.Name,
                    userCreatedId = x.UserCreatedId,
                    invitee = user.UserID != x.UserCreatedId ? x.InviteeName : null,
                    selected = request.CalendarIds.Contains(x.Id)
                }),
                defaultView = user.DefaultCalendarView,
                defaultNativeView = user.DefaultNativeCalendarView
            });
        }
    }
}
