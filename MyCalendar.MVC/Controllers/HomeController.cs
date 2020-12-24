
using MyCalendar.Enums;
using MyCalendar.Helpers;
using MyCalendar.Model;
using MyCalendar.Service;
using MyCalendar.Website.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MyCalendar.Controllers
{
    public class HomeController : UserMvcController
    {
        private readonly IDocumentService documentService;

        public HomeController(IUserService userService, IFeatureRoleService featureRoleService, IDocumentService documentService) : base(userService, featureRoleService)
        {
            this.documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
        }

        public async Task<ActionResult> Index(Status? updateResponse = null, string updateMsg = null)
        {
            await BaseViewModel(new MenuItem { Home = true }, updateResponse, updateMsg);
            return View();
        }

        public async Task<ActionResult> ChangeLog()
        {
            await BaseViewModel(new MenuItem { None = true });
            var changeLogDocs = await documentService.GetAllByTypeIdAsync((int)TypeIdentifier.ChangeLog);
            return View(changeLogDocs);
        }
    }
}