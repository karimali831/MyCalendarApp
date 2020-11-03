using MyCalendar.Controllers;
using MyCalendar.Model;
using MyCalendar.Service;
using MyCalendar.Website.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MyCalendar.Website.Controllers
{
    public class DocumentController : UserMvcController
    {
        private readonly IDocumentService documentService;

        public DocumentController(IDocumentService documentService, IUserService userService) : base(userService)
        {
            this.documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
        }

        // GET: Document
        public async Task<ActionResult> Index()
        {
            await BaseViewModel(new MenuItem { Documents = true });
            var baseVM = ViewData["BaseVM"] as BaseVM;

            var documents = await documentService.GetAllByUserIdAsync(baseVM.User.UserID);

            return View(new DocumentVM
            {
                Documents = documents ?? Enumerable.Empty<Document>()
            });
        }
    }
}