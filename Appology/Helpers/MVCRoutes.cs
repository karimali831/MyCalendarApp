﻿using Appology.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Configuration;

namespace Appology.Helpers
{
    public static class MVCSections
    {
        public static IList<string> Areas = new List<string> { 
            "calendar", 
            "write", 
            "errandrunner", 
            "admin", 
            "finance"
        };

        public static (string Controller, string Action, string Area) GetRouting(string url)
        {
            string controller;
            string action;
            string area;

            string[] route = url.Split('/');

            if (route.Length == 2)
            {
                area = "";
                controller = route[1];
                action = "index";
            }
            else
            {
                if (Areas.Contains(route[0]))
                {
                    area = route[1];
                    action = route.Length == 3 ? "index" : route[3];
                    controller = route[2];
                }
                else
                {
                    area = "";
                    action = route[2];
                    controller = route[1];
                }
            }

            return (controller, action, area);
        }


        private static IList<KeyValuePair<Section, string>> MvcRoutes()
        {
            return  new List<KeyValuePair<Section, string>>() {
                // base
                new KeyValuePair<Section, string>(Section.Login, "/account"),
                new KeyValuePair<Section, string>(Section.LoginSubmit, "/account/login"),
                new KeyValuePair<Section, string>(Section.Home, "/home"),
                new KeyValuePair<Section, string>(Section.Profile, "/account/settings"),
                new KeyValuePair<Section, string>(Section.Logout, "/account/logout"),
                new KeyValuePair<Section, string>(Section.Invite, "/invite/user"),
                new KeyValuePair<Section, string>(Section.RemoveBuddy, "/invite/remove"),
                // calendar app
                new KeyValuePair<Section, string>(Section.Scheduler, "/calendar/event/multiadd"),
                new KeyValuePair<Section, string>(Section.ActivityHub, "/calendar/activityhub"),
                new KeyValuePair<Section, string>(Section.ActivityHubFilter, "/calendar/activityhub/filter"),
                new KeyValuePair<Section, string>(Section.ActivityHubAdd, "/calendar/activityhub/add"),
                new KeyValuePair<Section, string>(Section.ActivityHubDelete, "/calendar/activityhub/delete"),
                new KeyValuePair<Section, string>(Section.CronofyProfiles, "/calendar/cronofy/profiles"),
                // write app
                new KeyValuePair<Section, string>(Section.Document, "/write/document"),
                new KeyValuePair<Section, string>(Section.DocLink, "/write/document/id"),
                // errand runner app
                new KeyValuePair<Section, string>(Section.ErrandRunnerNewOrder, "/errandrunner/order/new"),
                new KeyValuePair<Section, string>(Section.ErrandRunnerDriverApp, "/errandrunner/driver"),
                // finance app
                new KeyValuePair<Section, string>(Section.FinanceAppAddSpending, "/finance/app/addspending"),
                new KeyValuePair<Section, string>(Section.FinanceApp, "/finance/app"),
                new KeyValuePair<Section, string>(Section.FinanceCategories, "/finance/app/categories"),
                new KeyValuePair<Section, string>(Section.FinanceSettings, "/finance/app/settings"),
                new KeyValuePair<Section, string>(Section.Monzo, "/finance/Monzo/ApproveDataAccess"),
                new KeyValuePair<Section, string>(Section.MonzoAuthenticate, "/finance/Monzo/Login"),
                // admin
                new KeyValuePair<Section, string>(Section.Cache, "/admin/cache"),
                new KeyValuePair<Section, string>(Section.Dashboard, "/admin/app")
            };
        }

        public static string InviterShareLink(this UrlHelper helper, Guid userId)
        {
            var inviteRoute = MvcRoute(helper, Section.Invite);
            return $"{ConfigurationManager.AppSettings["RootUrl"]}/{inviteRoute.ControllerName}/{inviteRoute.ActionName}/{userId}";
        }

        public static string DocumentShareLink(this UrlHelper helper, Guid docId)
        {
            var documentRoute = MvcRoute(helper, Section.DocLink);
            return $"{ConfigurationManager.AppSettings["RootUrl"]}/{documentRoute.AreaName}/{documentRoute.ControllerName}/{documentRoute.ActionName}/{docId}";
        }

        public static string MvcRouteUrl(this UrlHelper helper, Section route)
        {
            return MvcRoutes().First(x => x.Key == route).Value;
        }

        public static (string AreaName, string ActionName, string ControllerName, string RouteUrl) MvcRoute(this UrlHelper helper, Section route)
        {
            var getSection = MvcRoutes().First(x => x.Key == route);
            var getRoute = GetRouting(getSection.Value);
            return (getRoute.Area, getRoute.Action, getRoute.Controller, getSection.Value);
        }

        public static object Login(this UrlHelper helper, Guid? inviteeId = null, Guid? docId = null)
        {
            var route = MvcRoute(helper, Section.Login);

            return new { 
                action = route.ActionName,
                controller = route.ControllerName, 
                area = "",
                inviteeId = inviteeId,
                docId = docId
            };
        }

        public static object Home(this UrlHelper helper, Status? updateResponse = null, string updateMsg = null)
        {
            return new {
                action = MvcRoute(helper, Section.Home).ActionName, 
                controller = MvcRoute(helper, Section.Home).ControllerName,
                area = "",
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
                area = "",
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
                id = inviteeId,
                area = "",
            };
        }
    }
}
