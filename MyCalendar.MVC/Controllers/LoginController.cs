using MyCalendar.Controllers;
using MyCalendar.Helpers;
using MyCalendar.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MyCalendar.Website.Controllers
{
    public class LoginController : UserMvcController
    {
        public LoginController(IUserService userService) : base(userService)
        {

        }

        [Route("Login/Index/{inviteeId?}")]
        public ActionResult Index(Guid? inviteeId = null)
        {
            TempData["InviteeId"] = inviteeId;
            return View();
        }

        [ValidateAntiForgeryToken()]
        [HttpPost]
        public async Task<ActionResult> Index(string input, Guid? inviteeId = null)
        {
            if (int.TryParse(input, out int passcode))
            {
                var user = await GetUser(passcode);

                if (user == null)
                {
                    TempData["ErrorMsg"] = "The passcode was entered incorrectly";
                    return RedirectToRoute(Url.Login(inviteeId));
                }
                else
                {
                    Response.SetCookie(new HttpCookie(AuthenticationName, passcode.ToString()));

                    if (inviteeId.HasValue)
                    {
                        return RedirectToRoute(Url.Invite(inviteeId.Value));
                    }

                    return RedirectToRoute(Url.Home());
                }
            }
            else
            {
                TempData["ErrorMsg"] = "Passcode must only contain numbers";
                return RedirectToRoute(Url.Login(inviteeId));
            }
        }
    }
}