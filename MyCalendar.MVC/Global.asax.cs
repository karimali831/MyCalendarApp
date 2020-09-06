using DFM.ExceptionHandling;
using DFM.ExceptionHandling.Sentry;
using System;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace MyCalendar
{
    public class MvcApplication : HttpApplication
    {
        private readonly IExceptionHandlerService exceptionHandlerService;

        public MvcApplication()
        {
            this.exceptionHandlerService = new ExceptionHandlerService(ConfigurationManager.AppSettings["DFM.ExceptionHandling.Sentry.Environment"]);
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        public void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs
            var exception = Context.Server.GetLastError() ?? Context.AllErrors.FirstOrDefault();

            if (exception != null)
            {
                // Update to Sentry
                exceptionHandlerService.ReportException(exception).Submit();
            }
        }
    }
}
