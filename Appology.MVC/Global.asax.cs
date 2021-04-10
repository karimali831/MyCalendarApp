using DFM.ExceptionHandling;
using DFM.ExceptionHandling.Sentry;
using StackExchange.Profiling;
using StackExchange.Profiling.EntityFramework6;
using StackExchange.Profiling.Mvc;
using System;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Appology
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
            //GlobalFilters.Filters.Add(new RequireHttpsAttribute());
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            GlobalFilters.Filters.Add(new ProfilingActionFilter());
            MiniProfiler.Configure(new MiniProfilerOptions
            {
                SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter(),
                ResultsAuthorize = request => request.IsLocal
            }
            .AddViewProfiling());

            MiniProfilerEF6.Initialize();
        }

        protected void Application_BeginRequest()
        {
            {
                MiniProfiler.StartNew();
            }
        }

        protected void Application_EndRequest()
        {
            MiniProfiler.Current?.Stop(discardResults: false);
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
