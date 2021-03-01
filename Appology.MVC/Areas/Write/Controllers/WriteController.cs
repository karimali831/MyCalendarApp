using Appology.Controllers;
using Appology.Service;
using Appology.Website.ViewModels;
using Appology.Write.Service;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Appology.Areas.Write.Controllers
{
    public class WriteController : UserMvcController
    {
        private readonly IDocumentService documentService;

        public WriteController(
            IUserService userService,
            IDocumentService documentService,
            IFeatureRoleService featureRoleService,
            INotificationService notificationService) : base(userService, featureRoleService, notificationService)
        {
            this.documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
   
        }
         
        public async Task<ActionResult> Index()
        {
            await BaseViewModel(new MenuItem { Documents = true });
            return View();

        }
    }
}