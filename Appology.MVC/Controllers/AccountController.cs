using Appology.Controllers;
using Appology.Helpers;
using Appology.Model;
using Appology.Security;
using Appology.Service;
using Appology.Website.Areas.MiFinance.ViewModels;
using Appology.Website.ViewModels;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Appology.Controllers
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
            return View(new LoginViewModel());
        }

        //[ValidateAntiForgeryToken()]
        //[HttpPost]
        //public async Task<ActionResult> Register()
        //{ 

        //}


        [ValidateAntiForgeryToken()]
        [HttpPost]
        public async Task<ActionResult> Login(LoginViewModel model)
        {

            if (string.IsNullOrEmpty(model.Email) ||
                string.IsNullOrEmpty(model.Password) ||
                await GetUser(model.Email, model.Password) == null)
            {
                TempData["ErrorMsg"] = "Login failed";
                return Json(new { status = false, url = Url.Login(model.InviteeId) });
            }
            SessionPersister.Email = model.Email;
        
            if (model.InviteeId.HasValue)
            {
                return Json(new { status = true, url = Url.Invite(model.InviteeId.Value) });
            }

            return Json(new { status = true, url = Url.Home() });
        }

        public ActionResult Logout()
        {
            SessionPersister.Email = string.Empty;
            return RedirectToRoute(Url.Login());
        }
    }
}