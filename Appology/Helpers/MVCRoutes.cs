using Appology.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Configuration;

namespace Appology.Helpers
{
    public static class MVCSections
    {
        public static string GetActionName(string url) => url.Split('/').Last();
        public static string GetControllerName(string url) => url.Split('/').Reverse().Skip(1).First();
        public static string GetAreaName(string url) => url.Split('/').Length == 2 ? url.Split('/')[0] : null;

        public static IList<KeyValuePair<Section, string>> MvcRoutes()
        {
            return  new List<KeyValuePair<Section, string>>() {
                // base
                new KeyValuePair<Section, string>(Section.Login, "/account/index"),
                new KeyValuePair<Section, string>(Section.LoginSubmit, "/account/login"),
                new KeyValuePair<Section, string>(Section.Home, "/home/index"),
                new KeyValuePair<Section, string>(Section.Profile, "/account/settings"),
                new KeyValuePair<Section, string>(Section.Logout, "/account/logout"),
                new KeyValuePair<Section, string>(Section.Invite, "/invite/user"),
                new KeyValuePair<Section, string>(Section.RemoveBuddy, "/invite/remove"),
                // calendar and write app
                new KeyValuePair<Section, string>(Section.Overview, "/calendar/event/overview"),
                new KeyValuePair<Section, string>(Section.Scheduler, "/calendar/event/multiadd"),
                new KeyValuePair<Section, string>(Section.CronofyProfiles, "/calendar/cronofy/profiles"),
                new KeyValuePair<Section, string>(Section.Document, "/calendar/document/index"),
                new KeyValuePair<Section, string>(Section.MoveDocument, "/calendar/document/move"),
                // errand runner app
                new KeyValuePair<Section, string>(Section.ErrandRunnerNewOrder, "/errandrunner/order/new"),
                // finance app
                new KeyValuePair<Section, string>(Section.FinanceApp, "/finance/app"),
                new KeyValuePair<Section, string>(Section.FinanceCategories, "/finance/app/categories"),
                new KeyValuePair<Section, string>(Section.FinanceSettings, "/finance/app/settings"),
                new KeyValuePair<Section, string>(Section.Monzo, "/finance/Monzo/ApproveDataAccess"),
                new KeyValuePair<Section, string>(Section.MonzoAuthenticate, "/finance/Monzo/Login")
            };
        }

        public static string InviterShareLink(this UrlHelper helper, Guid userId)
        {
            var inviteRoute = MvcRoute(helper, Section.Invite);
            return $"{ConfigurationManager.AppSettings["RootUrl"]}/{inviteRoute.ControllerName}/{inviteRoute.ActionName}/{userId}";
        }

        public static string MvcRouteUrl(this UrlHelper helper, Section route)
        {
            return MvcRoutes().First(x => x.Key == route).Value;
        }

        public static (string ActionName, string ControllerName, string RouteUrl) MvcRoute(this UrlHelper helper, Section route)
        {
            var getSection = MvcRoutes().First(x => x.Key == route);
            return (GetActionName(getSection.Value), GetControllerName(getSection.Value), getSection.Value);
        }

        public static object Login(this UrlHelper helper, Guid? inviteeId = null)
        {
            var route = MvcRoute(helper, Section.Login);

            return new { 
                action = route.ActionName,
                controller = route.ControllerName, 
                inviteeId = inviteeId
            };
        }

        public static object Home(this UrlHelper helper, Status? updateResponse = null, string updateMsg = null)
        {
            return new {
                action = MvcRoute(helper, Section.Home).ActionName, 
                controller = MvcRoute(helper, Section.Home).ControllerName,
                updateResponse,
                updateMsg
            };
        }

        public static object Settings(this UrlHelper helper, Status? updateResponse = null, string updateMsg = null)
        {
            return new
            {
                action = MvcRoute(helper, Section.Profile).ActionName,
                controller = MvcRoute(helper, Section.Profile).ControllerName,
                updateResponse,
                updateMsg
            };
        }

        public static object CronofyProfiles(this UrlHelper helper, Status? updateResponse = null, string updateMsg = null)
        {
            return new
            {
                action = MvcRoute(helper, Section.CronofyProfiles).ActionName,
                controller = MvcRoute(helper, Section.CronofyProfiles).ControllerName,
                updateResponse,
                updateMsg,
                area = "Calendar"
            };
        }

        public static object Scheduler(this UrlHelper helper)
        {
            return new { 
                action = MvcRoute(helper, Section.Scheduler).ActionName, 
                controller = MvcRoute(helper, Section.Scheduler).ControllerName,
                area = "Calendar"
            };
        }

        public static object Invite(this UrlHelper helper, Guid inviteeId)
        {
            return new { 
                action = MvcRoute(helper, Section.Invite).ActionName, 
                controller = MvcRoute(helper, Section.Invite).ControllerName, 
                id = inviteeId 
            };
        }
    }
}
