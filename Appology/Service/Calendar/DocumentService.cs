using Appology.MiCalendar.Model;
using Appology.MiCalendar.Repository;
using Appology.Enums;
using Appology.Helpers;
using Appology.Model;
using Appology.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Appology.MiCalendar.Helpers;

namespace Appology.MiCalendar.Service
{
    public interface IDocumentService
    {
        Task<IEnumerable<Document>> GetAllByUserIdAsync(Guid userId);
        Task<Document> GetAsync(Guid Id);
        Task<IEnumerable<Document>> GetAllByTypeIdAsync(int typeId);
        Task<IEnumerable<Types>> GetDocumentFoldersByUserIdAsync(Guid userId);
        Task<bool> InsertOrUpdateAsync(Document doc);
        Task<bool> MoveAsync(Guid docId, int moveToId);
        Task<bool> UpdateLastViewedDoc(Guid userId, Guid docId);
        Task<IList<Notification>> DocumentActivity(User user);
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

        public async Task<IEnumerable<Document>> GetAllByUserIdAsync(Guid userId)
        {
            return await documentRepository.GetAllByUserIdAsync(userId);
        }

        public async Task<IEnumerable<Types>> GetDocumentFoldersByUserIdAsync(Guid userId)
        {
            var documentFolders = new List<Types>();

            var documentTypes = (await typeService.GetAllAsync())
                .Where(x => x.GroupId == TypeGroup.DocumentFolders && (x.UserCreatedId == userId || (x.InviteeIdsList != null && x.InviteeIdsList.Contains(userId))));

            if (documentTypes != null && documentTypes.Any())
            {
                foreach (var docType in documentTypes)
                {
                    var type = (await typeService.GetAllByUserIdAsync(docType.UserCreatedId)).FirstOrDefault(x => x.Id == docType.Id);

                    if (type != null)
                    {
                        type.InviteeName = (await userRepo.GetByUserIDAsync(type.UserCreatedId))?.Name ?? "buddy";
                        documentFolders.Add(type);
                    }
                }
            }

            return documentFolders;
        }

        public async Task<IEnumerable<Document>> GetAllByTypeIdAsync(int typeId)
        {
            var docs =  await documentRepository.GetAllByTypeIdAsync(typeId);

            if (docs != null && docs.Any())
            {
                foreach (var doc in docs)
                {
                    doc.UserCreatedName = (await userRepo.GetByUserIDAsync(doc.UserCreatedId)).Name;

                    if (doc.EditedById.HasValue)
                    {
                        string editedByName = (await userRepo.GetByUserIDAsync(doc.EditedById.Value)).Name;
                        doc.EditedByName = $"{editedByName} on {DateUtils.FromUtcToTimeZone(doc.EditedDate.Value):dd-MM-yy HH:mm}";
                    }
                }
            }

            return docs;
        }

        public async Task<Document> GetAsync(Guid Id)
        {
            return await documentRepository.GetAsync(Id);
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
                var doc = await GetAsync(user.LastViewedDocId.Value);

                activity.Add(new Notification
                {
                    Avatar = CalendarUtils.AvatarSrc(user.UserID, user.Avatar, user.Name),
                    Text = $"You recently viewed a document: {doc.Title}",
                    Feature = Features.Write
                });
            }

            return activity;
        }
    }
}
