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
        private readonly ITypeService typeService;

        public DocumentController(
            IDocumentService documentService, 
            IUserService userService, 
            ITypeService typeService,
            IFeatureRoleService featureRoleService) : base(userService, featureRoleService)
        {
            this.documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
            this.typeService = typeService ?? throw new ArgumentNullException(nameof(typeService));
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

        public async Task<JsonResult> Move(string Id, int moveToId)
        {
            bool status = false;
            string responseText = "";
            if (Guid.TryParse(Id, out Guid docId))
            {
                var menuItem = new MenuItem { Settings = true };
                await BaseViewModel(menuItem);
                var baseVM = ViewData["BaseVM"] as BaseVM;

                var document = await documentService.GetAsync(docId);
                var type = await typeService.GetAsync(moveToId);

                if (baseVM.User.UserID != document.UserCreatedId)
                {
                    status = false;
                    responseText = "The document you're attempting to move was not created by you";
                }

                else if (baseVM.User.UserID != type.UserCreatedId)
                {
                    status = false;
                    responseText = "The document you're trying to move cannot be moved because it's contained in a folder not created by you";
                }
                else
                {
                    status = await documentService.MoveAsync(docId, moveToId);
                }
            }

            return new JsonResult { Data = new { status, responseText }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
    }

        public async Task<ActionResult> FolderSelection(Guid Id, string name)
        {
            await BaseViewModel(new MenuItem { Documents = true });
            var baseVM = ViewData["BaseVM"] as BaseVM;

            var folders = await documentService.GetDocumentFoldersByUserIdAsync(baseVM.User.UserID);

            var model = new DocumentMoveVM
            {
                Type = (Id.ToString(), name),
                UserTypes = folders,
                IsDocument = true
            };

            return PartialView("_FolderSelection", model);
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