
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
        protected readonly string AuthenticationName = "iCalendarApp-Authentication";
        public BaseVM BaseVM { get; set; }

        public UserMvcController(IUserService userService)
        {
            this.exceptionHandlerService = new ExceptionHandlerService(ConfigurationManager.AppSettings["DFM.ExceptionHandling.Sentry.Environment"]);
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            //userService.DoWorkAsync();
        }

        private async Task<BaseVM> getViewModel(MenuItem menuItem, Status? updateResponse = null, string updateMsg = null)
        {
            var user = await userService.GetUser();
            var users = await userService.GetUsers();
            var userTags = await userService.GetUserTags();

            string defaultCalendar = null;
            if (user != null && user.CronofyReady)
            {
                var getDefaultCalendar = userService.GetCalendars().FirstOrDefault(x => x.CalendarId == user.DefaultCalendar)?.Profile;

                if (getDefaultCalendar != null && !string.IsNullOrEmpty(getDefaultCalendar.ProviderName))
                {
                    defaultCalendar = Utils.UppercaseFirst(getDefaultCalendar.ProviderName);
                }
            }

            return new BaseVM
            {
                User = user,
                Users = users,
                UserTags = new TagsDTO { Tags = userTags },
                UpdateStatus = (updateResponse, updateMsg),
                MenuItem = menuItem,
                DefaultCalendarProvider = defaultCalendar
            };
        }

        public async Task<IList<string>> CurrentUserActivity(IEnumerable<Event> events)
        {
            return await userService.CurrentUserActivity(events);
        }

        public async Task<User> GetUser(int? passcode = null)
        {
            return await userService.GetUser(passcode);
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
                    filterContext.Result = new RedirectResult("/home");
                }
            }

            base.OnActionExecuting(filterContext);
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var appCookie = Request.Cookies.Get(AuthenticationName);

            if (appCookie == null)
            {
                if (!string.IsNullOrEmpty(HttpContext.Request.RawUrl) && HttpContext.Request.RawUrl != "/home/login")
                {
                    filterContext.Result = new RedirectResult("/home/login");
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(HttpContext.Request.RawUrl) && HttpContext.Request.RawUrl.Equals("/home/login", StringComparison.InvariantCultureIgnoreCase))
                {
                    filterContext.Result = new RedirectResult("/home/index");
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
                filterContext.Result = new RedirectResult("/home");
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