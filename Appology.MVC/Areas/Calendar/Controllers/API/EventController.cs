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
    [EnableCors(origins: "http://localhost:3000", headers: "*", methods: "*")]
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

        [Route("save")]
        [HttpPost]
        public async Task<HttpResponseMessage> SaveEvent(EventDTO dto)
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
            var save = await eventService.SaveGetEvent(e);

            if (save != null)
            {
                var getEvent = MapDTO(new List<Event> { save }, user.UserID).FirstOrDefault();
                return Request.CreateResponse(HttpStatusCode.OK, getEvent);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, false);
            }
        }

        [Route("retainselection")]
        [HttpPost]
        public async Task<HttpResponseMessage> RetainCalendarSelection(int[] calendarIds)
        {
            var user = await GetUser();
            var status = await userService.RetainCalendarSelection(calendarIds, user.UserID);
            return Request.CreateResponse(HttpStatusCode.OK, status);
        }

        [Route("retainview/{view}")]
        [HttpGet]
        public async Task<HttpResponseMessage> RetainCalendarView(string view)
        {
            var user = await GetUser();
            var status = await userService.RetainCalendarView(view, user.UserID);
            return Request.CreateResponse(HttpStatusCode.OK, status);
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
                editable = false, // x.UserID == userId draggable or resizeable api not implemented
                backgroundColor = x.ThemeColor ?? "lightslategrey",
                //display = x.UserID != userId ? "list-item" : "block",
                //textColor = !string.IsNullOrEmpty(x.ThemeColor) ? Utils.ContrastColor(x.ThemeColor) : null,
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

            // retain selection checked
            if (user.SelectedCalendarsList != null && user.SelectedCalendarsList.Any())
            {
                request.CalendarIds = request.CalendarIds.Length > 0 ? request.CalendarIds : userCalendars
                    .Where(x => user.SelectedCalendarsList.Contains(x.Id))
                    .Select(x => x.Id)
                    .ToArray();

                if (!Enumerable.SequenceEqual(user.SelectedCalendarsList, request.CalendarIds))
                {
                    await userService.RetainCalendarSelection(request.CalendarIds, user.UserID);
                }
            }
            // retain selection unchecked
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
                Events = MapDTO(events, user.UserID), 
                UserCalendars = userCalendars.Select(x => new
                {
                    id = x.Id,
                    name = x.Name,
                    userCreatedId = x.UserCreatedId,
                    invitee = user.UserID != x.UserCreatedId ? x.InviteeName : null,
                    selected = request.CalendarIds.Contains(x.Id)
                }),
                retainSelection = user.SelectedCalendarsList != null && user.SelectedCalendarsList.Any(),
                retainView = user.SelectedCalendarView,
                UserId = user.UserID
            });
        }
    }
}
