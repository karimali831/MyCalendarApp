using MyCalendar.Enums;
using MyCalendar.Model;
using MyCalendar.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyCalendar.Service
{
    public interface IDocumentService
    {
        Task<IEnumerable<Document>> GetAllByUserIdAsync(Guid userId);
        Task<Document> GetAsync(Guid Id);
    }

    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository documentRepository;

        public DocumentService(IDocumentRepository documentRepository)
        {
            this.documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
        }

        public async Task<IEnumerable<Document>> GetAllByUserIdAsync(Guid userId)
        {
            return await documentRepository.GetAllByUserIdAsync(userId);
        }

        public async Task<Document> GetAsync(Guid Id)
        {
            return await documentRepository.GetAsync(Id);
        }
    }
}
