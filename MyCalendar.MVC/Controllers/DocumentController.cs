using MyCalendar.Controllers;
using MyCalendar.DTOs;
using MyCalendar.Helpers;
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

        public DocumentController(IDocumentService documentService, IUserService userService, IFeatureRoleService featureRoleService) : base(userService, featureRoleService)
        {
            this.documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
        }

        // GET: Document
        public async Task<ActionResult> Index()
        {
            await BaseViewModel(new MenuItem { Documents = true });
            var baseVM = ViewData["BaseVM"] as BaseVM;

            var documents = await documentService.GetAllByUserIdAsync(baseVM.User.UserID);
            var folders = await documentService.GetDocumentFoldersByUserIdAsync(baseVM.User.UserID);

            return View(new DocumentVM
            {
                UserId = baseVM.User.UserID,
                Documents = documents ?? Enumerable.Empty<Document>(),
                UserFolders = folders
            });
        }

        public async Task<JsonResult> Get(int typeId)
        {
            var documents = await documentService.GetAllByTypeIdAsync(typeId);
            return new JsonResult { Data = documents, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public async Task<JsonResult> InsertOrUpdate(DocumentDTO dto)
        {
            var user = await GetUser();
            var document = (Document)null;

            if (dto.Id.HasValue)
            {
                document = await documentService.GetAsync(dto.Id.Value);
            }

            var model = new Document
            {
                Id = dto.Id ?? Guid.NewGuid(),
                TypeId = dto.TypeId,
                Title = dto.Title,
                Text = dto.Text,
                UserCreatedId = dto.Id.HasValue ? document.UserCreatedId : user.UserID,
                CreatedDate = dto.Id.HasValue ? document.CreatedDate : Utils.FromTimeZoneToUtc(Utils.DateTime()),
                EditedById = user.UserID
            };

            var status = await documentService.InsertOrUpdateAsync(model);
            return new JsonResult { Data = new { status } };
        }
    }
}