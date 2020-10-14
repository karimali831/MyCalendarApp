using Cronofy;
using MyCalendar.Controllers;
using MyCalendar.Enums;
using MyCalendar.Helpers;
using MyCalendar.Service;
using MyCalendar.Website.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MyCalendar.Website.Controllers
{
    public class CronofyController : UserMvcController
    {

        public CronofyController(ICronofyService cronofyService, IUserService userService, ITagService tagService) : base(userService, cronofyService, tagService)
        {
        }

        public async Task<ActionResult> Auth()
        {
            var user = await GetUser();
            var token = cronofyService.GetOAuthToken(Request.QueryString["code"]);
            cronofyService.SetToken(token);

            var account = cronofyService.GetAccount();

            if (account == null || user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            user.CronofyUid = account.Id;
            user.AccessToken = token.AccessToken;
            user.RefreshToken = token.RefreshToken;

            await UpdateUser(user);

            return RedirectToAction("Profiles");
        }

        public async Task<ActionResult> Profiles(Status? updateResponse = null, string updateMsg = null)
        {
            await BaseViewModel(new MenuItem { None = true }, updateResponse, updateMsg);

            var profiles = new Dictionary<Profile, Calendar[]>();
            var calendars = cronofyService.GetCalendars();

            foreach (var profile in cronofyService.GetProfiles())
            {
                profiles.Add(profile, calendars.Where(x => x.Profile.ProfileId == profile.Id).ToArray());
            }

            ViewData["MenuItem"] = new MenuItem { Home = true };
            return View("Profiles", new CronofyVM { Profiles = profiles } );
        }

        public ActionResult Calendar(string id)
        {
            var calendar = cronofyService.GetCalendars().First(x => x.CalendarId == id);
            var events = cronofyService.ReadEventsForCalendar(id).ToList();
            ViewData["MenuItem"] = new MenuItem { Home = true };

            return View("Calendar", new CronofyVM { Calendar = calendar, Events = events });
        }

        public ActionResult Event(string id)
        {
            var shownEvent = cronofyService.ReadEvents().First(x => x.EventUid == id);

            ViewData["calendarName"] = cronofyService.GetCalendars().First(x => x.CalendarId == shownEvent.CalendarId).Name;
            ViewData["google_maps_embed_api_key"] = ConfigurationManager.AppSettings["google_maps_embed_api_key"];
            ViewData["MenuItem"] = new MenuItem { Home = true };

            return View(new CronofyVM { Event = shownEvent });
        }

        [HttpGet]
        public ActionResult NewEvent([Bind(Prefix = "id")] string calendarId)
        {
            var newEvent = new EventVM
            {
                Calendar = cronofyService.GetCalendars().First(x => x.CalendarId == calendarId),
                CalendarId = calendarId,

                EventId = "unique_event_id_" + (new Random().Next(0, 1000000).ToString("D6"))
            };

            ViewData["MenuItem"] = new MenuItem { Home = true };

            return View(new CronofyVM { EventVM = newEvent });
        }

        [HttpPost]
        public ActionResult NewEvent(CronofyVM calendarVM)
        {
            var newEvent = calendarVM.EventVM;

            if (newEvent.Start > newEvent.End)
            {
                ModelState.AddModelError("End", "End time cannot be before start time");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    cronofyService.UpsertEvent(newEvent.EventId, newEvent.CalendarId, newEvent.Summary, newEvent.Description, newEvent.Start, newEvent.End, new Location(newEvent.LocationDescription, newEvent.Latitude, newEvent.Longitude));
                }
                catch (CronofyResponseException ex)
                {
                    newEvent.SetError(ex);
                }

                if (newEvent.NoErrors())
                {
                    return new RedirectResult(String.Format("/cronofy/calendar/{0}", newEvent.CalendarId));
                }
            }

            newEvent.Calendar = cronofyService.GetCalendars().First(x => x.CalendarId == newEvent.CalendarId);

            return View("NewEvent", newEvent);
        }

        public ActionResult DeleteExtEvent(CronofyVM calendarVM)
        {
            var deleteEvent = calendarVM.Event;
            cronofyService.DeleteExtEvent(deleteEvent.CalendarId, deleteEvent.EventUid);

            return new RedirectResult(String.Format("/cronofy/calendar/{0}", deleteEvent.CalendarId));
        }

        public ActionResult DeleteEvent(CronofyVM calendarVM)
        {
            var deleteEvent = calendarVM.Event;
            cronofyService.DeleteEvent(deleteEvent.CalendarId, deleteEvent.EventId);

            return new RedirectResult(String.Format("/cronofy/calendar/{0}", deleteEvent.CalendarId));
        }

        public async Task<ActionResult> UnlinkCalendar()
        {
            await BaseViewModel(new MenuItem { Overview = true });
            var baseVM = ViewData[nameof(BaseVM)] as BaseVM;

            baseVM.User.CronofyUid = null;
            baseVM.User.AccessToken = null;
            baseVM.User.RefreshToken = null;
            baseVM.User.DefaultCalendar = null;

            (Status? UpdateResponse, string UpdateMsg) = await UpdateUser(baseVM.User) == true
                ? (Status.Success, "Default calendar successfully set")
                : (Status.Failed, "There was an issue with updating your default calendar");

            return RedirectToAction("Settings", "Home", new { updateResponse = UpdateResponse, updateMsg = UpdateMsg });
        }

        public async Task<ActionResult> UpdateDefaultCalendar(string defaultCalendar)
        {
            await BaseViewModel(new MenuItem { Overview = true });
            var baseVM = ViewData[nameof(BaseVM)] as BaseVM;
            baseVM.User.DefaultCalendar = defaultCalendar;

            (Status? UpdateResponse, string UpdateMsg) = await UpdateUser(baseVM.User) == true
                ? (Status.Success, "Default calendar successfully set")
                : (Status.Failed, "There was an issue with updating your default calendar");

             return RedirectToAction("Profiles", new { updateResponse = UpdateResponse, updateMsg = UpdateMsg });
        }
    }
}