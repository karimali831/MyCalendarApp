
using Appology.Enums;
using Appology.Service;
using Appology.Website.Areas.MiCalendar.ViewModels;
using Appology.Website.ViewModels;
using Appology.Write.Service;
using StackExchange.Profiling;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Appology.Controllers
{
    public class HomeController : UserMvcController
    {
        private readonly IDocumentService documentService;

        public HomeController(
            IUserService userService,
            IDocumentService documentService,
            IFeatureRoleService featureRoleService,
            INotificationService notificationService) : base(userService, featureRoleService, notificationService)
        {
            this.documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
        }

        public async Task<ActionResult> Index(Status? updateResponse = null, string updateMsg = null)
        {
            await BaseViewModel(new MenuItem { Home = true }, updateResponse, updateMsg);
            var baseVM = ViewData["BaseVM"] as BaseVM;

            return View(
                new CalendarVM { 
                    UserCalendars = await UserCalendars(baseVM.User.UserID, userCreated: true) 
                }
            );
        }

        public async Task<ActionResult> ChangeLog()
        {
            await BaseViewModel(new MenuItem { None = true });
            var baseVM = ViewData["BaseVM"] as BaseVM;

            var changeLogDocs = await documentService.GetAllByTypeIdAsync((int)TypeIdentifier.ChangeLog);
            return View(changeLogDocs);
        }
    }
}