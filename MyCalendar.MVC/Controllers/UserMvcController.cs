
using DFM.ExceptionHandling;
using DFM.ExceptionHandling.Sentry;
using MyCalendar.DTOs;
using MyCalendar.Enums;
using MyCalendar.Helpers;
using MyCalendar.Model;
using MyCalendar.Security;
using MyCalendar.Service;
using MyCalendar.Website.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MyCalendar.Controllers
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

            return new BaseVM
            {
                User = user,
                Notifications = await Notifications(),
                Buddys = buddys,
                AccessibleGroups = await AccessibleGroups(user.RoleIdsList),
                AccessibleFeatures = await AccessibleFeatures(user.RoleIdsList),
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

            var getNotifications = await notificationService.GetNotifications(user, userCalendars);
            string notifications = "";

            if (getNotifications != null && getNotifications.Any())
            {
                int i = 1;
                foreach (var n in getNotifications)
                {
                    string icon = "";

                    switch (n.Feature)
                    {
                        case Features.Calendar:
                            icon = "<i class='fas fa-calendar-day'></i>";
                            break;
                        case Features.Write:
                            icon = "<i class='fas fa-edit'></i>";
                            break;
                        case Features.ErrandRunner:
                            icon = "<i class='fas fa-running></i>";
                            break;
                    }

                    if (i != 1)
                    {
                        notifications += "<hr />";
                    }
                    if (n.Avatar.Length == 2)
                    {
                        //notifications += $"<p class='pull-right' default-avatar='{n.Avatar}'> {icon} {n.Text}</p>";
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

        public async Task<User> Login(string email, string password)
        {
            return await userService.GetUser(email, password);
        }

        public async Task<User> GetUser()
        {
            return await userService.GetUser();
        }

        public async Task<IEnumerable<User>> GetUsers()
        {
            return await userService.GetAllAsync();
        }

        public async Task<IEnumerable<User>> GetBuddys(Guid userId)
        {
            return await userService.GetBuddys(userId);
        }

        public async Task<User> GetUserById(Guid userId)
        {
            return await userService.GetByUserIDAsync(userId);
        }

        public async Task<IEnumerable<Group>> AccessibleGroups(IEnumerable<Guid> roleIds)
        {
            return await featureRoleService.AccessibleGroups(roleIds);
        }

        public async Task<IEnumerable<Feature>> AccessibleFeatures(IEnumerable<Guid> roleIds)
        {
            return await featureRoleService.AccessibleFeatures(roleIds);
        }

        public async Task<bool> UpdateUser(User user)
        {
            return await userService.UpdateAsync(user);
        }

        public async Task<bool> UpdateUserTags(IEnumerable<Tag> tags, Guid userID)
        {
            return await userService.UpdateUserTagsAsync(tags, userID);
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var appCookie = SessionPersister.Email;
            var loginRoute = Url.MvcRoute(Section.Login);

            if (string.IsNullOrEmpty(appCookie))
            {
                string controller = RouteData.Values["controller"].ToString();
                string action = RouteData.Values["action"].ToString();
                string id = RouteData.Values["id"]?.ToString() ?? null;

                if (!controller.UnstrictCompare(loginRoute.ControllerName) || !action.UnstrictCompare(loginRoute.ActionName))
                {
                    string invitee = null;
                    var inviteRoute = Url.MvcRoute(Section.Invite);

                    if (controller.UnstrictCompare(inviteRoute.ControllerName) && action.UnstrictCompare(inviteRoute.ActionName))
                    {
                        invitee = $"?inviteeId={id}";
                    }

                    HttpContext.Response.Redirect(loginRoute.RouteUrl + invitee);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(HttpContext.Request.RawUrl) && (HttpContext.Request.RawUrl.Equals(loginRoute.RouteUrl, StringComparison.InvariantCultureIgnoreCase) || HttpContext.Request.RawUrl == "/"))
                {
                    HttpContext.Response.Redirect(Url.MvcRoute(Section.Home).RouteUrl);
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