using Cronofy;
using Appology.Controllers;
using Appology.Enums;
using Appology.Helpers;
using Appology.Model;
using Appology.Service;
using Appology.Website.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using Appology.MiCalendar.Service;
using Appology.MiCalendar.Enums;
using Appology.MiCalendar.Model;
using Appology.Website.Areas.MiCalendar.ViewModels;

namespace Appology.Areas.MiCalendar.Controllers
{
    public class CronofyController : UserMvcController
    {
        private readonly ICronofyService cronofyService;
        private readonly IUserService userService;

        public CronofyController(
            ICronofyService cronofyService,
            IUserService userService,
            IFeatureRoleService featureRoleService,
            INotificationService notificationService) : base(userService, featureRoleService, notificationService)
        {
            this.cronofyService = cronofyService ?? throw new ArgumentNullException(nameof(cronofyService));
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            
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
                var status = await userService.UpdateCronofyUserCredentials(account.Id, token.AccessToken, token.RefreshToken, user.UserID);

                if (status)
                {
                    updateResponse = Status.Success;
                    updateMsg = "External calendars successfully linked to Appology Calendar";
                }
                else
                {
                    updateResponse = Status.Failed;
                    updateMsg = "An error occured linking account to Appology Calendar";
                }
            }

            return RedirectToRoute(Url.CronofyProfiles(updateResponse, updateMsg));
        }

        public async Task<ActionResult> Profiles(Status? updateResponse = null, string updateMsg = null)
        {
            await BaseViewModel(new MenuItem { Cronofy = true }, updateResponse, updateMsg);
            var baseVM = ViewData["BaseVM"] as BaseVM;
            var profiles = new Dictionary<Profile, Cronofy.Calendar[]>();

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
                    UserCalendars = await UserCalendars(baseVM.User.UserID, userCreated: true),
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

            (Status? UpdateResponse, string UpdateMsg) = await userService.UpdateCronofyCalendarRights(rights.Values, baseVM.User.UserID) == true
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

            var unsetExtCalendars = await userService.UpdateCronofyUserCredentials(null, null, null, baseVM.User.UserID);
            var unsetExtCalendarRights = await userService.UpdateCronofyCalendarRights(null, baseVM.User.UserID);

            (Status? UpdateResponse, string UpdateMsg) = unsetExtCalendars && unsetExtCalendarRights
                ? (Status.Success, "External calendars successfully unlinked from Appology Calendar")
                : (Status.Failed, "There was an issue unlinking the external Calendars");

            return RedirectToRoute(Url.CronofyProfiles(UpdateResponse, UpdateMsg));
        }
    }
}