using Appology.Controllers;
using Appology.Service;
using Appology.Website.ViewModels;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Appology.Areas.ER.Controllers
{
    public class OrderController : UserMvcController
    {
        public OrderController(
            IUserService userService,
            IFeatureRoleService featureRoleService,
            INotificationService notificationService) : base(userService, featureRoleService, notificationService)
        {

        }
        public async Task<ActionResult> New()
        {
            await BaseViewModel(new MenuItem { ERNewOrder = true });
            return View();
        }
    }
}