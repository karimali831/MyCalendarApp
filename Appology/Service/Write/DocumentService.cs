using Appology.Enums;
using Appology.Helpers;
using Appology.Model;
using Appology.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Appology.MiCalendar.Helpers;
using Appology.Write.Model;
using Appology.Write.Repository;
using Appology.Write.DTOs;
using Appology.Service;
using System.Web;
using System.Web.Mvc;

namespace Appology.Write.Service
{
    public interface IDocumentService
    {
        Task<Document> LoadDocument(Guid docId, Guid userId);
        Task<IList<(Guid Id, string Title)>> GetDocTitlesByFolderId(int typeId, Guid userId);
        Task<IEnumerable<Document>> GetAllByTypeIdAsync(int typeId);
        Task<bool> InsertOrUpdateAsync(Document doc);
        Task<bool> MoveAsync(Guid docId, int moveToId);
        Task<bool> UpdateLastViewedDoc(Guid userId, Guid docId);
        Task<IList<Notification>> DocumentActivity(User user);
        Task<bool> DocumentsExistsInGroup(int groupId);
        Task<bool> DeleteDocument(Guid docId);
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


        public async Task<IList<(Guid Id, string Title)>> GetDocTitlesByFolderId(int typeId, Guid userId)
        {
            return await documentRepository.GetDocTitlesByFolderId(typeId, userId);
        }

        public async Task<Document> LoadDocument(Guid docId, Guid userId)
        {
            var doc = await documentRepository.GetAsync(docId, userId);

            if (doc == null)
            {
                return null;
            }

            var docCreator = await userRepo.GetByUserIDAsync(doc.UserCreatedId);
            doc.UserCreatedName = docCreator.Name;

            if (doc.EditedById.HasValue)
            {
                var collaborator = await userRepo.GetByUserIDAsync(doc.EditedById.Value);
                doc.EditedByName = $"{collaborator.Name} on {DateUtils.FromUtcToTimeZone(doc.EditedDate.Value):dd-MM-yy HH:mm}";
            }

            var collaborators = new List<Collaborator>() { 
                new Collaborator
                {
                    Name = docCreator.Name,
                    Avatar = CalendarUtils.AvatarSrc(docCreator.UserID, docCreator.Avatar, docCreator.Name),
                    Title = "Creator"
                }
            };

            if (doc.InviteeIdsList.Any() && !doc.InviteeIdsList.Contains(docCreator.UserID))
            {
                foreach (var invitee in doc.InviteeIdsList)
                {
                    var collaborator = await userRepo.GetByUserIDAsync(invitee);

                    collaborators.Add(
                        new Collaborator
                        {
                            Name = collaborator.Name,
                            Avatar = CalendarUtils.AvatarSrc(invitee, collaborator.Avatar, collaborator.Name),
                            Title = "Invitee"
                        }
                    );
                }
            }

            doc.Collaborators = collaborators;
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

        public async Task<bool> UpdateLastViewedDoc(Guid userId, Guid docId)
        {
            return await userRepo.UpdateLastViewedDoc(userId, docId);
        }

        public async Task<IList<Notification>> DocumentActivity(User user)
        {
            var activity = new List<Notification>();

            if (user.LastViewedDocId != null && user.LastViewedDocId != Guid.Empty)
            {
                var doc = await LoadDocument(user.LastViewedDocId.Value, user.UserID);
                var url = new UrlHelper(HttpContext.Current.Request.RequestContext);

                if (doc != null)
                {
                    activity.Add(new Notification
                    {
                        Avatar = CalendarUtils.AvatarSrc(user.UserID, user.Avatar, user.Name),
                        Text = $"You recently viewed a document: <a href='{url.DocumentShareLink(doc.Id)}'>{doc.Title}</a>",
                        Feature = Features.Write
                    });
                }
            }

            return activity;
        }

        public async Task<bool> DocumentsExistsInGroup(int groupId)

        {
            return await documentRepository.DocumentsExistsInGroup(groupId);
        }
    }
}
