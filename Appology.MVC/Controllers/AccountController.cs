using Appology.Enums;
using Appology.Helpers;
using Appology.Security;
using Appology.Service;
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

        [Route("Account/Index/{inviteeId?}/{errorMsg?}")]
        public ActionResult Index(Guid? inviteeId = null, string errorMsg = null)
        {
            return View(new LoginViewModel{ 
                InviteeId = inviteeId, 
                ErrorMsg = errorMsg 
            } );
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(string email, string password, Guid? inviteeId = null)
        {
            string redirectUrl = "";

            if (string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password) ||
                await GetUser(email, password) == null)
            {
                redirectUrl = Url.MvcRouteUrl(Section.Login) + $"?inviteeId={inviteeId}&errorMsg=Login failed";
            }
            else
            {
                SessionPersister.Email = email;

                if (inviteeId.HasValue)
                {
                    var (UpdateResponse, UpdateMsg) = await AddBuddy(email, inviteeId.Value);
                    redirectUrl = Url.MvcRouteUrl(Section.Home) + $"?updateResponse={UpdateResponse}&updateMsg={UpdateMsg}";
                }
                else
                {
                    redirectUrl = Url.MvcRouteUrl(Section.Home);
                }
            }

            return Json(new { url = redirectUrl });

            //return new JsonResult
            //{
            //    Data = new
            //    {
            //        status = response.Status,
            //        url = response.RedirectUrl
            //    }
            //};
        }

        public ActionResult Logout()
        {
            SessionPersister.Email = string.Empty;
            return RedirectToRoute(Url.Login());
        }

        public async Task<ActionResult> Settings()
        {
            await BaseViewModel(new MenuItem { Settings = true });
            return View();
        }
    }
}