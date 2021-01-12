using MyCalendar.Controllers;
using MyCalendar.Helpers;
using MyCalendar.Model;
using MyCalendar.Security;
using MyCalendar.Service;
using MyCalendar.Website.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MyCalendar.Website.Controllers
{
    public class AccountController : UserMvcController
    {
        public AccountController(
            IUserService userService, 
            IFeatureRoleService featureRoleService,
            INotificationService notificationService) : base(userService, featureRoleService, notificationService)
        {
        }

        [Route("Account/Index/{inviteeId?}")]
        public ActionResult Index(Guid? inviteeId = null)
        {
            TempData["InviteeId"] = inviteeId;
            return View();
        }

        //[ValidateAntiForgeryToken()]
        //[HttpPost]
        //public async Task<ActionResult> Register()
        //{ 

        //}

        [ValidateAntiForgeryToken()]
        [HttpPost]
        public async Task<ActionResult> Index(string email, string password, Guid? inviteeId = null)
        {

            if (string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password) ||
                await Login(email, password) == null)
            {
                TempData["ErrorMsg"] = "Login failed";
                return RedirectToRoute(Url.Login(inviteeId));
            }
            SessionPersister.Email = email;
        
            if (inviteeId.HasValue)
            {
                return RedirectToRoute(Url.Invite(inviteeId.Value));
            }

            return RedirectToRoute(Url.Home());
        }

        public ActionResult Logout()
        {
            SessionPersister.Email = string.Empty;
            return RedirectToRoute(Url.Login());
        }
    }
}