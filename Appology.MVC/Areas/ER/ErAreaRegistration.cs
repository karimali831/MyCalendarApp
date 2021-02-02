using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Appology.Website.Areas.Calendar
{
    public class ErAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "ER";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.Routes.MapMvcAttributeRoutes();

            context.MapRoute(
                "er_default",
                "errandrunner/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                new { controller = "Order" },
                new[] { "Appology.Areas.ER.Controllers" }
            );
        }
    }
}