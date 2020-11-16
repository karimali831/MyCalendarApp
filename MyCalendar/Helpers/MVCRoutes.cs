using MyCalendar.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Configuration;

namespace MyCalendar.Helpers
{
    public static class MVCSections
    {
        public static string GetActionName(string url) => url.Split('/')[2];
        public static string GetControllerName(string url) => url.Split('/')[1];

        public static IList<KeyValuePair<Section, string>> MvcRoutes()
        {
            return  new List<KeyValuePair<Section, string>>() {
                new KeyValuePair<Section, string>(Section.Login, "/account/index"),
                new KeyValuePair<Section, string>(Section.Home, "/home/index"),
                new KeyValuePair<Section, string>(Section.Profile, "/settings/index"),
                new KeyValuePair<Section, string>(Section.UpdateTags, "/settings/updatetags"),
                new KeyValuePair<Section, string>(Section.UpdateType, "/settings/updatetype"),
                new KeyValuePair<Section, string>(Section.RemoveType, "/settings/removetype"),
                new KeyValuePair<Section, string>(Section.AddType, "/settings/addtype"),
                new KeyValuePair<Section, string>(Section.Logout, "/account/logout"),
                new KeyValuePair<Section, string>(Section.Overview, "/event/overview"),
                new KeyValuePair<Section, string>(Section.Scheduler, "/event/multiadd"),
                new KeyValuePair<Section, string>(Section.CronofyProfiles, "/cronofy/profiles"),
                new KeyValuePair<Section, string>(Section.Document, "/document/index"),
                new KeyValuePair<Section, string>(Section.Invite, "/invite/user"),
                new KeyValuePair<Section, string>(Section.RemoveBuddy, "/invite/remove"),
                new KeyValuePair<Section, string>(Section.ErrandRunnerNewOrder, "/errandrunner/neworder")
            };
        }

        public static string InviterShareLink(this UrlHelper helper, Guid userId)
        {
            var inviteRoute = MvcRoute(helper, Section.Invite);
            return $"{ConfigurationManager.AppSettings["RootUrl"]}/{inviteRoute.ControllerName}/{inviteRoute.ActionName}/{userId}";
        }


        public static (string ActionName, string ControllerName, string RouteUrl) MvcRoute(this UrlHelper helper, Section route)
        {
            var getSection = MvcRoutes().FirstOrDefault(x => x.Key == route);
            return (GetActionName(getSection.Value), GetControllerName(getSection.Value), getSection.Value);
        }

        public static object Login(this UrlHelper helper, Guid? inviteeId = null)
        {
            var route = MvcRoute(helper, Section.Login);
            return new { action = route.ActionName, controller = route.ControllerName, inviteeId};
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
                updateMsg
            };
        }

        public static object Scheduler(this UrlHelper helper)
        {
            return new { action = MvcRoute(helper, Section.Scheduler).ActionName, controller = MvcRoute(helper, Section.Scheduler).ControllerName };
        }

        public static object Invite(this UrlHelper helper, Guid inviteeId)
        {
            return new { action = MvcRoute(helper, Section.Invite).ActionName, controller = MvcRoute(helper, Section.Invite).ControllerName, id = inviteeId };
        }
    }
}
