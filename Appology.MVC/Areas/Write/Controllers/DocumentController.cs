using Appology.Controllers;
using Appology.Helpers;
using Appology.Service;
using Appology.Website.ViewModels;
using Appology.Write.Service;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Appology.Areas.Write.Controllers
{
    public class DocumentController : UserMvcController
    {
        public DocumentController(
            IUserService userService,
            IFeatureRoleService featureRoleService,
            INotificationService notificationService) : base(userService, featureRoleService, notificationService)
        {
        }
         
        public async Task<ActionResult> Index()
        {
            await BaseViewModel(new MenuItem { Documents = true }); 
            return View();
        }

        public async Task<ActionResult> Id(Guid id)
        {
            await BaseViewModel(new MenuItem { Documents = true });
            return View("Index");
        }
    }
}