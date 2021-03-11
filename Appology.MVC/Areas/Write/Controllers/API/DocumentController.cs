using Appology.Helpers;
using Appology.Service;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Appology.Model;
using Appology.Controllers.Api;
using System.Configuration;
using Appology.Enums;
using Appology.Write.Model;
using Appology.Write.Service;
using Appology.Write.DTOs;
using System.Collections.Generic;

namespace Appology.Areas.Write.Controllers.API
{
    [RoutePrefix("api/document")]
    [CamelCaseControllerConfig]
    public class DocumentController : ApiController
    {
        private readonly ITypeService typeService;
        private readonly IDocumentService documentService;
        private readonly IUserService userService;
        private readonly string rootUrl = ConfigurationManager.AppSettings["RootUrl"];

        public DocumentController(ITypeService typeService, IUserService userService, IDocumentService documentService)
        {
            this.typeService = typeService ?? throw new ArgumentNullException(nameof(typeService));
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
        }

        private async Task<User> GetUser()
        {
            bool isLocal = this.rootUrl == "http://localhost:53822";
            return await userService.GetUser(isLocal ? "karimali831@googlemail.com" : null);
        }

        [Route("titles/{folderId}")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetDocumentTitlesByFolder(int folderId)
        {
            var user = await GetUser();
            var documents = await documentService.GetDocTitlesByFolderId(folderId, user.UserID);
            var recentDocs = await documentService.GetDocTitlesByDocIds(user.RecentOpenedDocIdsList);
            var pinnedDocs = await documentService.GetDocTitlesByDocIds(user.PinnedDocIdsList);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                documents,
                recentDocs,
                pinnedDocs
            });
        }

        [Route("search/{filter}")]
        [HttpGet]
        public async Task<HttpResponseMessage> SearchDocuments(string filter)
        {
            var user = await GetUser();
            var documents = await documentService.SearchDocuments(filter, user.UserID);

            return Request.CreateResponse(HttpStatusCode.OK, documents);
        }

        [Route("get/{docId}")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetDocument(Guid docId)
        {
            var user = await GetUser();
            var document = await documentService.LoadDocument(docId, user);
            return Request.CreateResponse(HttpStatusCode.OK, document);
        }

        [Route("pin/{docId}")]
        [HttpGet]
        public async Task<HttpResponseMessage> PinDocument(Guid docId)
        {
            var user = await GetUser();

            bool added;
            var pinnedDocIds = user.PinnedDocIdsList.ToList();

            if (pinnedDocIds.Contains(docId))
            {
                added = false;
                pinnedDocIds.Remove(docId);
            }
            else
            {
                added = true;
                pinnedDocIds.Add(docId);
            }

            var status = await documentService.PinDoc(user.UserID, string.Join(",", pinnedDocIds.Distinct()));
            return Request.CreateResponse(HttpStatusCode.OK, new { status, added });
        }

        [Route("folders")]
        [HttpGet]
        public async Task<HttpResponseMessage> UserDocumentFolders()
        {
            var user = await GetUser();
            var userTypes = await typeService.GetAllByUserIdAsync(user.UserID, TypeGroup.DocumentFolders, userCreatedOnly: false);
            var recentDocs = await documentService.GetDocTitlesByDocIds(user.RecentOpenedDocIdsList);
            var pinnedDocs = await documentService.GetDocTitlesByDocIds(user.PinnedDocIdsList);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                folders = userService.GetUserTypes(user, userTypes),
                recentDocs,
                pinnedDocs
            });
        }


        [Route("updaterecentdocs/{docId}")]
        [HttpGet]
        public async Task<HttpResponseMessage> UpdateRecentDocs(Guid docId)
        {
            var user = await GetUser();
            var doc = await documentService.LoadDocument(docId, user);

            if (doc != null)
            {
                var recentDocIds = user.RecentOpenedDocIdsList.ToList();
                recentDocIds.Add(docId);

                string docIds = string.Join(",", recentDocIds
                    .Skip(Math.Max(0, recentDocIds.Count() - 5))
                    .Distinct());

                var status = await documentService.UpdateRecentOpenedDocs(user.UserID, docIds);
                return Request.CreateResponse(HttpStatusCode.OK, status);
            }

            return null;
        }

        [Route("delete/{docId}")]
        [HttpGet]
        public async Task<HttpResponseMessage> DeleteDocument(Guid docId)
        {
            var user = await GetUser();
            var doc = await documentService.LoadDocument(docId, user);

            if (doc != null && doc.UserCreatedId == user.UserID)
            {
                return Request.CreateResponse(HttpStatusCode.OK, await documentService.DeleteDocument(docId));
            }

            return Request.CreateResponse(HttpStatusCode.OK, false);
        }

        [Route("move/{docId}/{moveToFolder}")]
        [HttpGet]
        public async Task<HttpResponseMessage> MoveDocument(Guid docId, int moveToFolder)
        {
            var user = await GetUser();
            var doc = await documentService.LoadDocument(docId, user);

            if (doc != null && doc.UserCreatedId == user.UserID)
            {
                return Request.CreateResponse(HttpStatusCode.OK, await documentService.MoveAsync(docId, moveToFolder));
            }

            return Request.CreateResponse(HttpStatusCode.OK, false);
        }

        [Route("save")]
        [HttpPost]
        public async Task<HttpResponseMessage> Save(DocumentDTO dto)
        {
            var user = await GetUser();
            var document = (Document)null;

            if (dto.Id.HasValue)
            {
                document = await documentService.LoadDocument(dto.Id.Value, user);
            }

            var model = new Document
            {
                Id = dto.Id ?? Guid.NewGuid(),
                TypeId = dto.TypeId,
                Title = dto.Title,
                Text = dto.Text,
                UserCreatedId = dto.Id.HasValue ? document.UserCreatedId : user.UserID,
                CreatedDate = dto.Id.HasValue ? document.CreatedDate : DateUtils.FromTimeZoneToUtc(DateUtils.DateTime()),
                EditedById = user.UserID
            };

            var status = await documentService.InsertOrUpdateAsync(model);

            if (!status)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, false);
            }

            return Request.CreateResponse(HttpStatusCode.OK, model);
        }

    }
}
