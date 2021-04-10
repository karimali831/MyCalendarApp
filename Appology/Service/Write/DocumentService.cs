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
using System.Web;
using System.Web.Mvc;
using System.Linq;
using Appology.Write.ViewModels;
using Appology.Write.DTOs;
using Appology.Service;

namespace Appology.Write.Service
{
    public interface IDocumentService
    {
        Task<Document> LoadDocument(Guid docId, User user);
        Task<IEnumerable<Document>> GetAllByTypeIdAsync(int typeId);
        Task<bool> InsertOrUpdateAsync(DocumentDTO doc);
        Task<bool> MoveAsync(Guid docId, int moveToId);
        Task<bool> UpdateRecentOpenedDocs(Guid userId, string docIds);
        Task<IList<Notification>> RecentViewedDocs(User user);
        Task<bool> DocumentsExistsInGroup(int groupId);
        Task<bool> DeleteDocument(Guid docId);
        Task<DocumentTitlesVM> GetDocumentTitle(Guid docId);
        Task<bool> PinDoc(Guid userId, string docIds);
        string LastViewedDocIds(IEnumerable<Guid> docIds, int take = 5);
        Task<IList<DocumentTitlesVM>> GetDocumentTitles(Guid userId);
        Task<IList<DocumentTitlesVM>> SearchDocumentsByFilter(string filter, Guid userId);
        Task<bool> InsertChangelog(DocumentChangelog docChangeLog);
        Task<(string OldText, string NewText)> GetDocChangelogTexts(int Id, Guid userId);
        Task<IEnumerable<(Guid DocId, string Tag)>> GetAllDocumentUserTags(Guid userId);
        Task<bool> DocumentExists(Guid docId, Guid userCreatedId); 
    }

    public class DocumentService : IDocumentService
    {
        public static readonly string cachePrefix = typeof(DocumentService).FullName;
        private readonly IUserRepository userRepo;
        private readonly IDocumentRepository documentRepository;
        private readonly IDocumentChangelogRepository documentChangelogRepository;
        private readonly ICacheService cache;

        public DocumentService(ICacheService cache, IDocumentRepository documentRepository, IUserRepository userRepo, IDocumentChangelogRepository documentChangelogRepository)
        {
            this.documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
            this.documentChangelogRepository = documentChangelogRepository ?? throw new ArgumentNullException(nameof(documentChangelogRepository));
            this.userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<IEnumerable<Document>> GetAllByTypeIdAsync(int typeId)
        {
            return await cache.GetAsync(
                $"{cachePrefix}.{nameof(GetAllByTypeIdAsync)}",
                async () => await documentRepository.GetAllByTypeIdAsync(typeId)
            );
        }

        public async Task<bool> DocumentExists(Guid docId, Guid userCreatedId)
        {
            return await documentRepository.DocumentExists(docId, userCreatedId);
        }

        public async Task<IList<DocumentTitlesVM>> SearchDocumentsByFilter(string filter, Guid userId)
        {
            var searchTags = (await GetAllDocumentUserTags(userId)).Select(x => x.DocId);
            return await documentRepository.SearchDocumentsByFilter(searchTags, filter, userId);
        }

        public async Task<IList<DocumentTitlesVM>> GetDocumentTitles(Guid userId)
        {
            return await cache.GetAsync(
                $"{cachePrefix}.{nameof(GetDocumentTitles)}",
                async () => await DocTitles(await documentRepository.GetDocumentTitles(userId))
            );
        }

        public async Task<DocumentTitlesVM> GetDocumentTitle(Guid docId)
        {
            var doc = new List<DocumentTitlesVM>() { await documentRepository.GetDocumentTitle(docId) };
            return (await DocTitles(doc)).First();
        }

        private async Task<IList<DocumentTitlesVM>> DocTitles(IList<DocumentTitlesVM> titles)
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
            return await cache.GetAsync(
                $"{cachePrefix}.{nameof(LoadDocument)}.{docId}",
                async () =>
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
                        doc.EditedBy = $"Edited {DateUtils.GetPrettyDate(doc.EditedDate.Value)} by {editor.Name}";
                    }

                    if (user.PinnedDocIdsList.Any())
                    {
                        doc.Pinned = user.PinnedDocIdsList.Contains(docId);
                    }

                    doc.Changelog = (await documentChangelogRepository.GetDocChangelogTitles(docId)) ?? Enumerable.Empty<DocumentChangelog>();

                    if (doc.Changelog.Any())
                    {
                        var collaborators = await userRepo.GetCollaboratorsAsync(doc.Changelog.Select(x => x.UserId));

                        doc.Changelog.Select(x =>
                        {
                            var editedBy = collaborators.FirstOrDefault(c => c.CollaboratorId == x.UserId);

                            x.EditedByAvatar = CalendarUtils.AvatarSrc(editedBy.CollaboratorId, editedBy.Avatar, editedBy.Name);
                            x.EditedBy = $"Edited {DateUtils.GetPrettyDate(x.Date)} by {editedBy.Name}";
                            x.EditedDate = x.Date.ToString("dd/MM/yyyy HH:mm");

                            return x;
                        })
                        .OrderByDescending(x => x.Date)
                        .ToList();
                    }

                    return doc;
                }
            );
        }

        public async Task<bool> DeleteDocument(Guid docId)
        {
            RemoveCache();
            return await documentRepository.DeleteDocument(docId);
        }

        public async Task<bool> InsertOrUpdateAsync(DocumentDTO doc)
        {
            RemoveCache();
            return await documentRepository.InsertOrUpdateAsync(doc);
        }

        public async Task<bool> MoveAsync(Guid docId, int moveToId)
        {
            RemoveCache();
            return await documentRepository.MoveAsync(docId, moveToId);
        }

        public async Task<bool> UpdateRecentOpenedDocs(Guid userId, string docIds)
        {
            return await userRepo.UpdateRecentOpenedDocs(userId, docIds);
        }

        public async Task<bool> PinDoc(Guid userId, string docIds)
        {
            RemoveCache();
            return await userRepo.PinDoc(userId, docIds);
        }

        public string LastViewedDocIds(IEnumerable<Guid> docIds, int take = 5)
        {
            return string.Join(",", docIds
                .Skip(Math.Max(0, docIds.Count() - take))
                .Distinct());
        }

        public async Task<IList<Notification>> RecentViewedDocs(User user)
        {
            var activity = new List<Notification>();

            // last viewed docs
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
                            TypeId = NotificationType.RecentlyViewedDocs
                        });
                    }
                }
            }

            return activity;
        }

        public async Task<bool> DocumentsExistsInGroup(int groupId)
        {
            return await documentRepository.DocumentsExistsInGroup(groupId);
        }

        public async Task<(string OldText, string NewText)> GetDocChangelogTexts(int Id, Guid userId)
        {
            return await documentChangelogRepository.GetDocChangelogTexts(Id, userId);
        }

        public async Task<bool> InsertChangelog(DocumentChangelog docChangeLog)
        {
            return await documentChangelogRepository.InsertChangelog(docChangeLog);
        }

        public async Task<IEnumerable<(Guid DocId, string Tag)>> GetAllDocumentUserTags(Guid userId)
        {
            return await documentRepository.GetAllDocumentUserTags(userId) ?? Enumerable.Empty<(Guid, string)>();
        }

        private void RemoveCache()
        {
            cache.RemoveAll(cachePrefix);
        }
    }
}
