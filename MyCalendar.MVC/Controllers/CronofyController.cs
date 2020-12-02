using Cronofy;
using MyCalendar.Controllers;
using MyCalendar.Enums;
using MyCalendar.Helpers;
using MyCalendar.Model;
using MyCalendar.Service;
using MyCalendar.Website.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MyCalendar.Website.Controllers
{
    public class CronofyController : UserMvcController
    {
        private readonly ICronofyService cronofyService;

        public CronofyController(IUserService userService, ICronofyService cronofyService, IFeatureRoleService featureRoleService) : base(userService, featureRoleService)
        {
            this.cronofyService = cronofyService ?? throw new ArgumentNullException(nameof(cronofyService));
            
        }

        public async Task<ActionResult> Auth(string code)
        {
            Status? updateResponse = null;
            string updateMsg = "";

            if (code == null)
            {
                updateResponse = Status.Failed;
                updateMsg = "Authorisation code cannot be empty";
                return RedirectToRoute(Url.CronofyProfiles(updateResponse, updateMsg));
            }

            var user = await GetUser();
            var token = cronofyService.GetOAuthToken(code);
            cronofyService.SetToken(token);

            var account = cronofyService.GetAccount();

            if (account == null)
            {
                updateResponse = Status.Failed;
                updateMsg = "No External calendar account found";
            }
            else
            {
                user.CronofyUid = account.Id;
                user.AccessToken = token.AccessToken;
                user.RefreshToken = token.RefreshToken;

                await UpdateUser(user);
            }

            return RedirectToRoute(Url.CronofyProfiles(updateResponse, updateMsg));
        }

        public async Task<ActionResult> Profiles(Status? updateResponse = null, string updateMsg = null)
        {
            await BaseViewModel(new MenuItem { Cronofy = true }, updateResponse, updateMsg);
            var baseVM = ViewData["BaseVM"] as BaseVM;
            var profiles = new Dictionary<Profile, Calendar[]>();

            if (baseVM.User.CronofyReady != CronofyStatus.NotAuthenticated && baseVM.User.CronofyReady != CronofyStatus.Disabled)
            { 
                var calendars = cronofyService.GetCalendars();

                foreach (var profile in cronofyService.GetProfiles())
                {
                    profiles.Add(profile, calendars.Where(x => x.Profile.ProfileId == profile.Id).ToArray());
                }
            }

            return View("Profiles", 
                new CronofyVM { 
                    UserCalendars = baseVM.UserCalendars,
                    Profiles = profiles, 
                    CronofyCalendarAuthUrl = cronofyService.GetAuthUrl() 
                } 
            );
        }

        private Expression<Func<string[], bool>> GetCalendarRights(string calendarId)
        {
            return permission => (permission.Where(x => x == calendarId).GroupBy(x => x).Any(g => g.Count() > 1) ? true : false);
        }

        [HttpPost]
        public async Task<ActionResult> Profiles(CronofyVM dto)
        {
            var rights = new Dictionary<int, ExtCalendarRights>();

            int a = 0;
            foreach (var item in dto.SyncFromCalendarId)
            {
                rights.Add(a, new ExtCalendarRights {
                    SyncFromCalendarId = item,
                    SyncToCalendarId = dto.SyncToCalendarId[a],
                    Read = dto.Read.Where(x => x == item).GroupBy(x => x).Any(g => g.Count() > 1),
                    Save = dto.Save.Where(x => x == item).GroupBy(x => x).Any(g => g.Count() > 1),
                    Delete = dto.Delete.Where(x => x == item).GroupBy(x => x).Any(g => g.Count() > 1)
                });
                    
                a++;
            }

            await BaseViewModel(new MenuItem { Cronofy = true });
            var baseVM = ViewData[nameof(BaseVM)] as BaseVM;
            baseVM.User.ExtCalendarRights = rights.Values;

            (Status? UpdateResponse, string UpdateMsg) = await UpdateUser(baseVM.User) == true
                ? (Status.Success, "Calendar rights has been set successfully")
                : (Status.Failed, "There was an issue with updating the Calendar rights");

            return RedirectToRoute(Url.CronofyProfiles(UpdateResponse, UpdateMsg));

        }

        public async Task<ActionResult> Calendar(string id)
        {
            await BaseViewModel(new MenuItem { Cronofy = true });

            var calendar = cronofyService.GetCalendars().First(x => x.CalendarId == id);
            var events = cronofyService.ReadEventsForCalendar(id).ToList();
            ViewData["MenuItem"] = new MenuItem { Home = true };

            return View("Calendar", new CronofyVM { Calendar = calendar, Events = events });
        }

        public async Task<ActionResult> Event(string id)
        {
            await BaseViewModel(new MenuItem { Cronofy = true });
            var shownEvent = cronofyService.ReadEvents().First(x => x.EventUid == id);

            ViewData["calendarName"] = cronofyService.GetCalendars().First(x => x.CalendarId == shownEvent.CalendarId).Name;
            ViewData["google_maps_embed_api_key"] = ConfigurationManager.AppSettings["google_maps_embed_api_key"];
            ViewData["MenuItem"] = new MenuItem { Home = true };

            return View(new CronofyVM { Event = shownEvent });
        }

        public async Task<ActionResult> UnlinkCalendar()
        {
            await BaseViewModel(new MenuItem { Cronofy = true });
            var baseVM = ViewData[nameof(BaseVM)] as BaseVM;

            baseVM.User.CronofyUid = null;
            baseVM.User.AccessToken = null;
            baseVM.User.RefreshToken = null;
            baseVM.User.ExtCalendars = null;
            baseVM.User.ExtCalendarRights = null;

            (Status? UpdateResponse, string UpdateMsg) = await UpdateUser(baseVM.User) == true
                ? (Status.Success, "External Calendars successfully unlinked")
                : (Status.Failed, "There was an issue unlinking the external Calendars");

            return RedirectToRoute(Url.CronofyProfiles(UpdateResponse, UpdateMsg));
        }
    }
}