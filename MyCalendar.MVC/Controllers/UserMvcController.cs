
using DFM.ExceptionHandling;
using DFM.ExceptionHandling.Sentry;
using MyCalendar.DTOs;
using MyCalendar.Enums;
using MyCalendar.Helpers;
using MyCalendar.Model;
using MyCalendar.Service;
using MyCalendar.Website.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Filters;
using System.Web.Mvc;

namespace MyCalendar.Controllers
{
    public class UserMvcController : Controller
    {
        private readonly IExceptionHandlerService exceptionHandlerService;
        private readonly IUserService userService; 
        protected readonly string AuthenticationName;
        public BaseVM BaseVM { get; set; }

        public UserMvcController(IUserService userService)
        {
            this.exceptionHandlerService = new ExceptionHandlerService(ConfigurationManager.AppSettings["DFM.ExceptionHandling.Sentry.Environment"]);
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));

            AuthenticationName = ConfigurationManager.AppSettings["AuthenticationName"];
            //userService.DoWorkAsync();
        }

        private async Task<BaseVM> getViewModel(MenuItem menuItem, Status? updateResponse = null, string updateMsg = null)
        {
            var user = await userService.GetUser();
            var buddys = await userService.GetBuddys(user.UserID);
            var userTags = await userService.GetUserTags(user.UserID);

            return new BaseVM
            {
                User = user,
                Buddys = buddys,
                UserTags = new TagsDTO { Tags = userTags },
                UpdateStatus = (updateResponse, updateMsg),
                MenuItem = menuItem
            };
        }

        public async Task<IList<string>> CurrentUserActivity(IEnumerable<Event> events, Guid userId)
        {
            return await userService.CurrentUserActivity(events, userId);
        }

        public async Task<User> GetUser(int? passcode = null)
        {
            return await userService.GetUser(passcode);
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

        public void LogoutUser()
        {
            var appCookie = Request.Cookies.Get(AuthenticationName);

            if (appCookie != null)
            {
                HttpCookie cookie = new HttpCookie(AuthenticationName)
                {
                    Expires = DateTime.Now.AddDays(-1)
                };
                Response.Cookies.Set(cookie);
            }
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
            var appCookie = Request.Cookies.Get(AuthenticationName);

            if (appCookie != null)
            {
                if (!userService.LoadUser(int.Parse(appCookie.Value)))
                {
                    Response.Cookies.Remove(AuthenticationName);
                    HttpContext.Response.Redirect(Url.MvcRoute(Section.Home).RouteUrl);
                }
            }

            base.OnActionExecuting(filterContext);
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var appCookie = Request.Cookies.Get(AuthenticationName);
            var loginRoute = Url.MvcRoute(Section.Login);

            if (appCookie == null)
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
                if (!string.IsNullOrEmpty(HttpContext.Request.RawUrl) && HttpContext.Request.RawUrl.Equals(loginRoute.RouteUrl, StringComparison.InvariantCultureIgnoreCase))
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
                Response.Cookies.Remove(AuthenticationName);
                HttpContext.Response.Redirect("/home");
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