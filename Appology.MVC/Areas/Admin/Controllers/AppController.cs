using Appology.Controllers;
using Appology.Service;
using Appology.Website.ViewModels;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Appology.Areas.Admin.Controllers
{
    public class AppController : UserMvcController
    {

        public AppController(
            IUserService userService,
            IFeatureRoleService featureRoleService,
            INotificationService notificationService) : base(userService, featureRoleService, notificationService)
        {
        }
         
        public async Task<ActionResult> Index()
        {
            await BaseViewModel(new MenuItem { Dashboard = true });

            return View();
        }

        public async Task<ActionResult> Users()
        {
            await BaseViewModel(new MenuItem { Dashboard = true });

            return View("Index");
        }
    }
}