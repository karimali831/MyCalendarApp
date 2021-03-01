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

namespace Appology.Areas.Write.Controllers.API
{
    [RoutePrefix("api/document")]
    [CamelCaseControllerConfig]
    public class WriteController : ApiController
    {
        private readonly ITypeService typeService;
        private readonly IDocumentService documentService;
        private readonly IUserService userService;
        private readonly string rootUrl = ConfigurationManager.AppSettings["RootUrl"];

        public WriteController(ITypeService typeService, IUserService userService, IDocumentService documentService)
        {
            this.typeService = typeService ?? throw new ArgumentNullException(nameof(typeService));
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
        }

        private async Task<User> GetUser()
        {
            bool isLocal = this.rootUrl == "http://localhost:53822";
            return await userService.GetUser(isLocal ? "davidedetomas@hotmail.com" : null);
        }

        [Route("titles/{folderId}/{filter?}")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetDocumentTitlesByFolder(int folderId, string filter = null)
        {
            var user = await GetUser();
            var documents = await documentService.GetDocTitlesByFolderId(folderId, user.UserID);

            return Request.CreateResponse(HttpStatusCode.OK, documents.Select(x => new { x.Id, x.Title }));
        }

        [Route("get/{docId}")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetDocument(Guid docId)
        {
            var user = await GetUser();
            var document = await documentService.LoadDocument(docId, user.UserID);
            return Request.CreateResponse(HttpStatusCode.OK, document);
        }

        [Route("folders/{filter?}")]
        [HttpGet]
        public async Task<HttpResponseMessage> UserDocumentFolders(string filter = null)
        {
            var user = await GetUser();
            var userTypes = await typeService.GetAllByUserIdAsync(user.UserID, TypeGroup.DocumentFolders, userCreatedOnly: false);

            return Request.CreateResponse(HttpStatusCode.OK, userService.GetUserTypes(user, userTypes));
        }

        [Route("delete/{docId}")]
        [HttpGet]
        public async Task<HttpResponseMessage> DeleteDocument(Guid docId)
        {
            var user = await GetUser();
            var doc = await documentService.LoadDocument(docId, user.UserID);

            if (doc != null && doc.UserCreatedId == user.UserID)
            {
                return Request.CreateResponse(HttpStatusCode.OK, await documentService.DeleteDocument(docId));
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
                document = await documentService.LoadDocument(dto.Id.Value, user.UserID);
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

            if (document == null)
            {
                document = await documentService.LoadDocument(model.Id, user.UserID);
            }

            return Request.CreateResponse(HttpStatusCode.OK, document);
        }

    }
}
