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
        private readonly IDocumentService documentService;

        public DocumentController(
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

        public async Task<ActionResult> Id(Guid id)
        {
            await BaseViewModel(new MenuItem { Documents = true });
            return View("Index");
        }
    }
}