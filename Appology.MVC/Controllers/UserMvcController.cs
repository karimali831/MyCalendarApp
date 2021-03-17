
using DFM.ExceptionHandling;
using DFM.ExceptionHandling.Sentry;
using Appology.Enums;
using Appology.Helpers;
using Appology.Model;
using Appology.Security;
using Appology.Service;
using Appology.Website.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Appology.MiCalendar.Model;
using Appology.MiCalendar.Service;
using Appology.Write.Service;

namespace Appology.Controllers
{
    public class UserMvcController : Controller
    {
        private readonly IExceptionHandlerService exceptionHandlerService;
        private readonly IUserService userService;
        private readonly IFeatureRoleService featureRoleService;
        private readonly INotificationService notificationService;
        protected readonly string AuthenticationName;
        public BaseVM BaseVM { get; set; }

        public UserMvcController(IUserService userService, IFeatureRoleService featureRoleService, INotificationService notificationService)
        {
            this.exceptionHandlerService = new ExceptionHandlerService(ConfigurationManager.AppSettings["DFM.ExceptionHandling.Sentry.Environment"]);
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.featureRoleService = featureRoleService ?? throw new ArgumentNullException(nameof(featureRoleService));
            this.notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));

            AuthenticationName = ConfigurationManager.AppSettings["AuthenticationName"];
        }

        private async Task<BaseVM> getViewModel(MenuItem menuItem, Status? updateResponse = null, string updateMsg = null)
        {
            var user = await userService.GetUser();
            var buddys = await userService.GetBuddys(user.UserID);
            var accessibleFeatures = await featureRoleService.AccessibleFeatures(user.RoleIdsList);
            var accessibleGroups = await featureRoleService.AccessibleGroups(user.RoleIdsList);

            return new BaseVM
            {
                User = user,
                Notifications = await Notifications(),
                Buddys = buddys,
                AccessibleGroups = accessibleGroups,
                AccessibleFeatures = accessibleFeatures,
                UpdateStatus = (updateResponse, updateMsg),
                MenuItem = menuItem
            };
        }

        public async Task<NotificationVM> Notifications()
        {
            var user = await GetUser();
            var userCalendars = (await UserCalendars(user.UserID))
                .Select(x => x.Id)
                .ToArray();

            var documentNotifications = await notificationService.DocumentNotifications(user);
            var eventNotifications = await notificationService.EventNotifications(user.UserID, userCalendars);
            var getNotifications = documentNotifications.Concat(eventNotifications);

            string notifications = "";

            if (getNotifications != null && getNotifications.Any())
            {
                int i = 1;
                foreach (var n in getNotifications)
                {
                    string icon = $"<i class='{n.FaIcon}'></i>";

                    if (i != 1)
                    {
                        notifications += "<hr />";
                    }

                    if (n.UserId != user.UserID || n.Avatar.Length == 2)
                    {
                        notifications += $"{icon} {n.Text}";
                    }
                    else
                    {
                        notifications += $"{icon} <img class='pull-right' width='30' height='30' src='{n.Avatar}'> {n.Text}";
    
                    }

                    i++;
                }
            }

            return new NotificationVM
            {
                Content = (Content(notifications, "text/html")),
                Count = getNotifications != null ? getNotifications.Count() : 0
            };
        }

        public async Task<IEnumerable<Types>> UserCalendars(Guid userId, bool userCreated = false)
        {
            return await userService.UserCalendars(userId, userCreated);
        }

        public async Task<IEnumerable<Tag>> UserTags(Guid userId)
        {
            return await userService.GetUserTags(userId);
        }

        public async Task<User> GetUser(string email, string password)
        {
            return await userService.GetUser(email, password);
        }

        public async Task<User> GetUser()
        {
            return await userService.GetUser();
        }

        public async Task<(Status? UpdateResponse, string UpdateMsg)> AddBuddy(string email, Guid id)
        {
            return await userService.AddBuddy(email, id);
        }


        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var appCookie = SessionPersister.Email;
            var loginRoute = Url.MvcRoute(Section.Login);
            var loginPostRoute = Url.MvcRoute(Section.LoginSubmit);

            if (string.IsNullOrEmpty(appCookie))
            {
                string controller = RouteData.Values["controller"].ToString();
                string action = RouteData.Values["action"].ToString();
                string id = RouteData.Values["id"]?.ToString() ?? null;

                if (!controller.UnstrictCompare(loginRoute.ControllerName) || !action.UnstrictCompare(loginRoute.ActionName))
                {
                    if (id != null)
                    {
                        var inviteRoute = Url.MvcRoute(Section.Invite);
                        var docRoute = Url.MvcRoute(Section.DocLink);


                        if (controller.UnstrictCompare(inviteRoute.ControllerName) && action.UnstrictCompare(inviteRoute.ActionName))
                        {
                            HttpContext.Response.Redirect(loginRoute.RouteUrl + $"?inviteeId={id}");
                        }

                        if (controller.UnstrictCompare(docRoute.ControllerName) && action.UnstrictCompare(docRoute.ActionName))
                        {
                            HttpContext.Response.Redirect(loginRoute.RouteUrl + $"?docId={id}");
                        }
                    }
                    else if (!controller.UnstrictCompare(loginPostRoute.ControllerName) && !action.UnstrictCompare(loginPostRoute.ActionName))
                    {
                        HttpContext.Response.Redirect(loginRoute.RouteUrl);
                    }
                }
            }

            base.OnActionExecuted(filterContext);
        }

        protected async virtual Task BaseViewModel(MenuItem menuItem, Status? updateResponse = null, string updateMsg = null)
        {
            ViewData[nameof(BaseVM)] = await getViewModel(menuItem, updateResponse, updateMsg);
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            var ex = filterContext.Exception;
            filterContext.ExceptionHandled = true;

            exceptionHandlerService.ReportException(ex).Submit();

            if (ex is CredentialsInvalidError)
            {
                SessionPersister.Email = string.Empty;
                HttpContext.Response.Redirect("/account");
                filterContext.ExceptionHandled = true;
            }

            filterContext.Result = new ViewResult
            {
                ViewName = "~/Views/Shared/Error.cshtml",
                ViewData = new ViewDataDictionary(filterContext.Controller.ViewData)
                {
                    Model = new ErrorVM { Exception = ex }
                }
            };

            base.OnException(filterContext);
        }
    }
}