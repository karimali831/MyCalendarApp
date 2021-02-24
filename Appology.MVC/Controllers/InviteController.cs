using Appology.Controllers;
using Appology.Enums;
using Appology.Helpers;
using Appology.Service;
using Appology.Website.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Appology.Controllers
{
    public class InviteController : UserMvcController
    {
        public InviteController(
            IUserService userService,
            IFeatureRoleService featureRoleService,
            INotificationService notificationService) : base(userService, featureRoleService, notificationService)
        {

        }

        public async Task<ActionResult> User(Guid id)
        {
            var user = await GetUser();

            if (user == null)
            {
                return RedirectToRoute(Url.Login(id));
            }
            else
            {
                var (UpdateResponse, UpdateMsg) = await AddBuddy(user.Email, id);
                return RedirectToRoute(Url.Home(UpdateResponse, UpdateMsg));
            }
        }
    }
}