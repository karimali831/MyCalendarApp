
using DFM.ExceptionHandling;
using DFM.ExceptionHandling.Sentry;
using MyCalendar.Website.ViewModels;
using System.Configuration;
using System.Web.Mvc;

namespace MyCalendar.Controllers
{
    public class UserMvcController : Controller
    {
        private readonly IExceptionHandlerService exceptionHandlerService;

        public UserMvcController()
        {
            this.exceptionHandlerService = new ExceptionHandlerService(ConfigurationManager.AppSettings["DFM.ExceptionHandling.Sentry.Environment"]);
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