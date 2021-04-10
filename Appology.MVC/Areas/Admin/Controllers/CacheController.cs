using Appology.Controllers;
using Appology.Helpers;
using Appology.Service;
using Appology.Website.Areas.Admin.ViewModels;
using Appology.Website.ViewModels;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Appology.Areas.Admin.Controllers
{
    public class CacheController : UserMvcController
    {
        private readonly ICacheService cacheService;

        public CacheController(
            IUserService userService,
            IFeatureRoleService featureRoleService,
            INotificationService notificationService,
            ICacheService cacheService) : base(userService, featureRoleService, notificationService)
        {
            this.cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }
         
        public async Task<ActionResult> Index()
        {
            await BaseViewModel(new MenuItem { Cache = true });

            var cacheKeys = cacheService.GetKeys();

            return View(new CacheVM { Keys = cacheKeys });
        }

        public ActionResult RemoveKey(string key)
        {
            cacheService.Remove(key);
            return RedirectToAction("Index");
        }

        public ActionResult RemoveAll()
        {
            cacheService.RemoveAll();
            return RedirectToAction("Index");
        }
    }
}