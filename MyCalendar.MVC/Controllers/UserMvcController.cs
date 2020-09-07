
using DFM.ExceptionHandling;
using DFM.ExceptionHandling.Sentry;
using MyCalendar.Model;
using MyCalendar.Service;
using MyCalendar.Website.ViewModels;
using System;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MyCalendar.Controllers
{
    public class UserMvcController : Controller
    {
        private readonly IExceptionHandlerService exceptionHandlerService;
        private readonly IUserService userService;
        private readonly string AuthenticationName = "iCalendarApp-Authentication";

        public UserMvcController(IUserService userService)
        {
            this.exceptionHandlerService = new ExceptionHandlerService(ConfigurationManager.AppSettings["DFM.ExceptionHandling.Sentry.Environment"]);
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public async Task<User> GetUser(int? passcode = null)
        {
            if (passcode.HasValue)
            {
                var user = await userService.GetAsync(passcode.Value);

                if (user != null)
                {
                    Session[AuthenticationName] = user.Passcode;
                    return user;
                }
            }
            else
            {
                if (Session[AuthenticationName] != null)
                {
                    var user = await userService.GetAsync(int.Parse(Session[AuthenticationName].ToString()));

                    if (user != null)
                    {
                        user.Authenticated = true;
                        return user;
                    }
                }
            }

            return null;
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            filterContext.ExceptionHandled = true;

            exceptionHandlerService.ReportException(filterContext.Exception).Submit();

            filterContext.Result = new ViewResult
            {
                ViewName = "~/Views/Shared/Error.cshtml",
                ViewData = new ViewDataDictionary(filterContext.Controller.ViewData)
                {
                    Model = new ErrorVM { Exception = filterContext.Exception }
                }
            };
        }
    }
}