using Appology.Controllers;
using Appology.Service;
using Appology.Website.ViewModels;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Appology.Areas.ER.Controllers
{
    #if !DEBUG
    [RequireHttps] //apply to all actions in controller
    #endif
    public class DriverController : UserMvcController
    {
        public DriverController(
            IUserService userService,
            IFeatureRoleService featureRoleService,
            INotificationService notificationService) : base(userService, featureRoleService, notificationService)
        {

        }
        public async Task<ActionResult> Index()
        {
            await BaseViewModel(new MenuItem { ERDriverApp = true });
            return View();
        }
    }
}