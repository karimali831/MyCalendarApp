using Appology.MiCalendar.Model;
using Appology.MiCalendar.Repository;
using System;
using System.Threading.Tasks;

namespace Appology.MiCalendar.Service
{
    public interface ITagService
    {
        Task<Tag> GetAsync(Guid tagId);
        Task<bool> EventsByTagExist(Guid tagID);
    }

    public class TagService : ITagService
    {
        private readonly ITagRepository tagRepository;
        private readonly IEventRepository eventRepository;

        public TagService(ITagRepository tagRepository, IEventRepository eventRepository)
        {
            this.tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(TagRepository));
            this.eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
        }

        public async Task<Tag> GetAsync(Guid tagId)
        {
            return await tagRepository.GetAsync(tagId);
        }

        public async Task<bool> EventsByTagExist(Guid tagID)
        {
            return await eventRepository.EventsByTagExist(tagID);
        }
    }
}
