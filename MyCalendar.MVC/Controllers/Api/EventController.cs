using Microsoft.AspNet.Identity;
using MyCalendar.DTOs;
using MyCalendar.ER.Model;
using MyCalendar.ER.Service;
using MyCalendar.Helpers;
using MyCalendar.Service;
using MyCalendar.Website.Controllers.Api;

using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace MyCalendar.Website.Controllers.API
{
    [RoutePrefix("api/calendar")]
    [EnableCors(origins: "http://localhost:3000", headers: "*", methods: "*")]
    [CamelCaseControllerConfig]
    public class EventController :  ApiController
    {
        private readonly IEventService eventService;
        private readonly IUserService userService;

        public EventController(IEventService eventService, IUserService userService)
        {
            this.eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        [Route("events")]
        [HttpPost]
        public async Task<HttpResponseMessage> Get(int[] calendarIds)
        {
            var user = await userService.GetUser("karimali831@googlemail.com");
            var userCalendars = await userService.UserCalendars(user.UserID);

            if (calendarIds.Length == 0)
            {
                calendarIds = userCalendars
                    .Where(x => x.UserCreatedId == user.UserID && x.Defaulted)
                    .Select(x => x.Id)
                    .ToArray();
            }

            var events = await eventService.GetAllAsync(user, calendarIds);
            var dto = events.Select(b => EventDTO.MapFrom(b)).ToList();

            var activeEvents = await eventService.GetCurrentActivityAsync();
            var currentActivity = await userService.CurrentUserActivity(activeEvents, user.UserID);

            return Request.CreateResponse(HttpStatusCode.OK, new { 
                Events = dto.Select(x => new
                {
                    // standard props
                    id = x.EventID,
                    title = string.IsNullOrEmpty(x.Subject) ? x.Description : x.Subject,
                    start = x.Start,
                    end = x.End,
                    startStr = x.Start.ToString("s", CultureInfo.InvariantCulture),
                    endStr = x.End.HasValue ? x.End.Value.ToString("s", CultureInfo.InvariantCulture) : null,
                    allDay = x.IsFullDay,
                    url = "",
                    classNames = "",
                    editable = true,
                    backgroundColor = x.ThemeColor,
                    display = x.UserID != user.UserID ? "list-item" : "block",
                    //textColor = !string.IsNullOrEmpty(x.ThemeColor) ? Utils.ContrastColor(x.ThemeColor) : null,
                    // non standard props
                    calendarId = x.CalendarId,
                    userId = x.UserID,
                    tagId = x.TagID,
                    description = x.Description,
                    tentative = x.Tentative,
                    duration = x.Duration,
                    eventUid = x.EventUid,
                    alarm = x.Alarm,
                    provider = x.Provider
                }), 
                UserCalendars = userCalendars.Select(x => new
                {
                    id = x.Id,
                    name = x.Name,
                    userCreatedId = x.UserCreatedId,
                    invitee = user.UserID != x.UserCreatedId ? x.InviteeName : null,
                    selected = calendarIds.Contains(x.Id)
                }),
                UserId = user.UserID,
                currentActivity,
            });
        }

    }
}
