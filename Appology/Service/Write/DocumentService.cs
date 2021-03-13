using Appology.Enums;
using Appology.Helpers;
using Appology.Model;
using Appology.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Appology.MiCalendar.Helpers;
using Appology.Write.Model;
using Appology.Write.Repository;
using Appology.Service;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using Appology.Write.ViewModels;
using Appology.Write.DTOs;

namespace Appology.Write.Service
{
    public interface IDocumentService
    {
        Task<Document> LoadDocument(Guid docId, User user);
        Task<IList<DocumentTitles>> GetDocTitlesByFolderId(int typeId, Guid userId);
        Task<IEnumerable<Document>> GetAllByTypeIdAsync(int typeId);
        Task<bool> InsertOrUpdateAsync(Document doc);
        Task<bool> MoveAsync(Guid docId, int moveToId);
        Task<bool> UpdateRecentOpenedDocs(Guid userId, string docIds);
        Task<IList<Notification>> DocumentActivity(User user);
        Task<bool> DocumentsExistsInGroup(int groupId);
        Task<bool> DeleteDocument(Guid docId);
        Task<bool> PinDoc(Guid userId, string docIds);
        Task<IList<DocumentTitles>> GetDocTitlesByDocIds(IEnumerable<Guid> docIds);
        string LastViewedDocIds(IEnumerable<Guid> docIds, int take = 5);
        Task<IList<DocumentTitles>> SearchDocuments(string filter, Guid userId);
    }

    public class DocumentService : IDocumentService
    {
        private readonly IUserRepository userRepo;
        private readonly IDocumentRepository documentRepository;
        private readonly ITypeService typeService;

        public DocumentService(IDocumentRepository documentRepository, ITypeService typeService, IUserRepository userRepo)
        {
            this.documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
            this.typeService = typeService ?? throw new ArgumentNullException(nameof(typeService));
            this.userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
        }

        public async Task<IEnumerable<Document>> GetAllByTypeIdAsync(int typeId)
        {
            return await documentRepository.GetAllByTypeIdAsync(typeId);
        }


        public async Task<IList<DocumentTitles>> GetDocTitlesByFolderId(int typeId, Guid userId)
        {
            return await DocTitles(await documentRepository.GetDocTitlesByFolderId(typeId, userId));
        }

        public async Task<IList<DocumentTitles>> GetDocTitlesByDocIds(IEnumerable<Guid> docIds)
        {
            return await DocTitles(await documentRepository.GetDocTitlesByDocIds(docIds));
        }

        public async Task<IList<DocumentTitles>> SearchDocuments(string filter, Guid userId)
        {
            return await documentRepository.SearchDocuments(filter, userId);
        }


        private async Task<IList<DocumentTitles>> DocTitles(IList<DocumentTitles> titles)
        {
            var collaborators = await userRepo.GetCollaboratorsAsync(titles.Select(x => x.EditedById));

            return titles
                .Select(x => {
                    var editedBy = collaborators.FirstOrDefault(c => c.CollaboratorId == x.EditedById);

                    x.LastedEditedDuration = DateUtils.Duration(DateUtils.DateTime(), x.EditedDate, incFollowingMeasures: false);
                    x.LastedEditedByUserAvatar =  CalendarUtils.AvatarSrc(editedBy.CollaboratorId, editedBy.Avatar, editedBy.Name);
                    
                    return x;
                })
                .OrderByDescending(x => x.EditedDate)
                .ToList();
        }

        public async Task<Document> LoadDocument(Guid docId, User user)
        {
            var doc = await documentRepository.GetAsync(docId, user.UserID);

            if (doc == null)
            {
                return null;
            }

            var docCreator = await userRepo.GetByUserIDAsync(doc.UserCreatedId);
            doc.UserCreatedName = docCreator.Name;

            if (doc.EditedById.HasValue)
            {
                var editor = await userRepo.GetByUserIDAsync(doc.EditedById.Value);
                var duration = DateUtils.Duration(DateUtils.DateTime(), doc.EditedDate.Value, incFollowingMeasures: false);

                doc.EditedBy = $"Last edited {(duration != "just now" ? "ago" : "")} by {editor.Name}";
            }

            if (user.PinnedDocIdsList.Any())
            {
                doc.Pinned = user.PinnedDocIdsList.Contains(docId);
            }

            return doc;
        }

        public async Task<bool> DeleteDocument(Guid docId)
        {
            return await documentRepository.DeleteDocument(docId);
        }

        public async Task<bool> InsertOrUpdateAsync(Document doc)
        {
            return await documentRepository.InsertOrUpdateAsync(doc);
        }

        public async Task<bool> MoveAsync(Guid docId, int moveToId)
        {
            return await documentRepository.MoveAsync(docId, moveToId);
        }

        public async Task<bool> UpdateRecentOpenedDocs(Guid userId, string docIds)
        {
            return await userRepo.UpdateRecentOpenedDocs(userId, docIds);
        }

        public async Task<bool> PinDoc(Guid userId, string docIds)
        {
            return await userRepo.PinDoc(userId, docIds);
        }

        public string LastViewedDocIds(IEnumerable<Guid> docIds, int take = 5)
        {
            return string.Join(",", docIds
                .Skip(Math.Max(0, docIds.Count() - take))
                .Distinct());
        }

        public async Task<IList<Notification>> DocumentActivity(User user)
        {
            var activity = new List<Notification>();

            // last viewed doc
            if (user.RecentOpenedDocIdsList.Any())
            {
                var lastViewedDocId = LastViewedDocIds(user.RecentOpenedDocIdsList, take: 1);

                if (Guid.TryParse(lastViewedDocId, out Guid docId))
                {
                    var url = new UrlHelper(HttpContext.Current.Request.RequestContext);
                    var doc = await LoadDocument(docId, user);

                    if (doc != null)
                    {
                        activity.Add(new Notification
                        {
                            Id = docId,
                            UserId = user.UserID,
                            Avatar = CalendarUtils.AvatarSrc(user.UserID, user.Avatar, user.Name),
                            Text = $"You last viewed: <a href='{url.DocumentShareLink(doc.Id)}'>{doc.Title}</a>",
                            FeatureId = Features.Write
                        });
                    }
                }
            }

            // pinned docs
            //if (user.PinnedDocIdsList != null && user.PinnedDocIdsList.Any())
            //{
            //    var url = new UrlHelper(HttpContext.Current.Request.RequestContext);

            //    var pinnedDocs = await GetDocTitlesByDocIds(user.PinnedDocIdsList);

            //    foreach (var doc in pinnedDocs)
            //    {
            //        activity.Add(new Notification
            //        {
            //            Id = doc.Id,
            //            Avatar = CalendarUtils.AvatarSrc(user.UserID, user.Avatar, user.Name),
            //            Text = $"Pinned document: <a href='{url.DocumentShareLink(doc.Id)}'>{doc.Title}</a> document.",
            //            FeatureId = Features.Write
            //        });
            //    }
            //}

            return activity;
        }

        public async Task<bool> DocumentsExistsInGroup(int groupId)

        {
            return await documentRepository.DocumentsExistsInGroup(groupId);
        }
    }
}
