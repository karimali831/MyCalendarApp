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
using Appology.Write.ViewModels;

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


        [Route("doctitlesandfolders")]
        [HttpGet]
        public async Task<HttpResponseMessage> UserDocumentFoldersAndTitles()
        {
            var user = await GetUser();
            var userTypes = await typeService.GetAllByUserIdAsync(user.UserID, TypeGroup.DocumentFolders, userCreatedOnly: false);
            var documents = await documentService.GetDocumentTitles(user.UserID);

            var tags = documents
                    .Where(x => !string.IsNullOrEmpty(x.Tags) && x.UserCreatedId == user.UserID)
                    .SelectMany(x => x.Tags.Split(','))
                    .Select(x => x);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                folders = userService.GetUserTypes(user, userTypes),
                documents,
                recentDocs = user.RecentOpenedDocIdsList,
                pinnedDocs = user.PinnedDocIdsList,
                tags
            });
        }

        [Route("search/{filter}")]
        [HttpGet]
        public async Task<HttpResponseMessage> SearchDocuments(string filter)
        {
            var user = await GetUser();
            var documents = await documentService.SearchDocumentsByFilter(filter, user.UserID);

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

        [Route("updaterecentdocs/{docId}")]
        [HttpGet]
        public async Task<HttpResponseMessage> UpdateRecentDocs(Guid docId)
        {
            var user = await GetUser();
            var doc = await documentService.LoadDocument(docId, user);

            if (doc != null)
            {
                string docIds = GetRecentDocs(user.RecentOpenedDocIdsList.ToList(), docId);

                var status = await documentService.UpdateRecentOpenedDocs(user.UserID, docIds);
                return Request.CreateResponse(HttpStatusCode.OK, status);
            }

            return Request.CreateResponse(HttpStatusCode.OK, false);
        }

        private string GetRecentDocs(IList<Guid> recentDocIds, Guid toAddDocId)
        {
            recentDocIds.Add(toAddDocId);

            return string.Join(",", recentDocIds
                .Skip(Math.Max(0, recentDocIds.Count() - 5))
                .Distinct());
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
                DocumentTitlesVM docTitle = null;

                // just return if no change to title and text
                if (dto.Text.Equals(document.Text) && dto.Title.Equals(document.Title) && dto.EditedAuto)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { status = true, doc = docTitle });
                }

                if (!dto.EditedAuto)
                {
                    if (!document.Text.Equals(dto.Text) || (document.DraftText != null && !document.DraftText.Equals(dto.Text)))
                    {
                        string oldText = !document.Text.Equals(dto.Text) ? document.Text : document.DraftText;

                        await documentService.InsertChangelog(new DocumentChangelog
                        {
                            DocId = dto.Id.Value,
                            UserId = user.UserID,
                            OldText = oldText,
                            NewText = dto.Text,
                            Date = DateUtils.DateTime()
                        });
                    }
                }
            }

            var model = dto;
            model.Id = dto.Id ?? Guid.NewGuid();
            model.DraftText = dto.EditedAuto ? document?.Text ?? null : null;
            model.UserCreatedId = document?.UserCreatedId ?? user.UserID;
            model.CreatedDate = document?.CreatedDate ?? DateUtils.FromTimeZoneToUtc(DateUtils.DateTime());
            model.EditedById = user.UserID;

            // update recent docs
            string docIds = GetRecentDocs(user.RecentOpenedDocIdsList.ToList(), model.Id.Value);
            await documentService.UpdateRecentOpenedDocs(user.UserID, docIds);

            var status = await documentService.InsertOrUpdateAsync(model);
            var doc = await documentService.GetDocumentTitle(model.Id.Value);

            return Request.CreateResponse(HttpStatusCode.OK, new { status, doc });
        }

        [Route("changelog/{Id}")]
        [HttpGet]
        public async Task<HttpResponseMessage> DocChangelog(int Id)
        {
            var user = await GetUser();
            var (OldText, NewText) = await documentService.GetDocChangelogTexts(Id, user.UserID);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                oldText = OldText,
                newText = NewText
            });
        }

    }
}
