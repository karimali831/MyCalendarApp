using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Appology.Website.Areas.Write
{
    public class WriteAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Write";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.Routes.MapMvcAttributeRoutes();

            context.MapRoute(
                "write_default",
                "Write/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                new { controller = "Write" },
                new[] { "Appology.Areas.Write.Controllers" }
            );
        }
    }
}