using MyCalendar.Model;
using MyCalendar.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MyCalendar.Security
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly IUserService userService;
        public CustomAuthorizeAttribute(IUserService userService)
        {
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public async override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (string.IsNullOrEmpty(SessionPersister.Email))
                filterContext.Result = new RedirectToRouteResult(new
                    RouteValueDictionary(new {
                        controller = "Account",
                        action = "Index"
                    }));
            else
            {
                var user = await userService.GetUser();
                CustomPrincipal mp = new CustomPrincipal(user);
                if (!mp.IsInRole(Roles))
                    filterContext.Result = new RedirectToRouteResult(new
                        RouteValueDictionary(new {
                            controller = "AccessDenied",
                            action = "Index"
                        }));
            }
        }
    }
}