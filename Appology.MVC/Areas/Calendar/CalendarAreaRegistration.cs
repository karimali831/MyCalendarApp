using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Appology.Website.Areas.MiCalendar
{
    public class CalendarAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Calendar";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.Routes.MapMvcAttributeRoutes();

            context.MapRoute(
                "calendar_default",
                "Calendar/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                new { controller = "Cronofy|Event|ActivityHub" },
                new[] { "Appology.Areas.MiCalendar.Controllers" }
            );
        }
    }
}