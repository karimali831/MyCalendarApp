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
        private readonly ICacheService cache;

        public AccountController(
            IUserService userService, 
            ICacheService cache,
            IFeatureRoleService featureRoleService,
            INotificationService notificationService) : base(userService, featureRoleService, notificationService)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public ActionResult Index(Guid? inviteeId = null, string errorMsg = null, Guid? docId = null)
        {
            return View(new LoginViewModel{ 
                InviteeId = inviteeId, 
                DocId = docId,
                ErrorMsg = errorMsg 
            } );
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(string email, string password, Guid? inviteeId = null, Guid? docId = null)
        {
            string redirectUrl = "";
            string invitee = inviteeId.HasValue ? $"&inviteeId={inviteeId}" : "";
            string doc = docId.HasValue ? $"&docId={docId}" : "";

            if (string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password) ||
                await GetUser(email, password) == null)
            {
                redirectUrl = Url.MvcRouteUrl(Section.Login) + $"?errorMsg=Login failed{invitee}{doc}";
            }
            else
            {
                SessionPersister.Email = email;

                if (inviteeId.HasValue)
                {
                    var (UpdateResponse, UpdateMsg) = await AddBuddy(email, inviteeId.Value);
                    redirectUrl = Url.MvcRouteUrl(Section.Home) + $"?updateResponse={UpdateResponse}&updateMsg={UpdateMsg}";
                }
                else if (docId.HasValue)
                {
                    redirectUrl = Url.MvcRouteUrl(Section.DocLink) + $"/{docId.Value}";
                }
                else
                {
                    redirectUrl = Url.MvcRouteUrl(Section.Home);
                }
            }

            return Json(new { url = redirectUrl });
        }

        public ActionResult Logout()
        {
            cache.RemoveAll();
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