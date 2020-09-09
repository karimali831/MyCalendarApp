using MyCalendar.Model;
using MyCalendar.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyCalendar.Service
{
    public interface ITagService
    {
        Task<IEnumerable<Tag>> GetUserTagsAsync(Guid userID);
        Task<bool> UpdateUserTagsAsync(IEnumerable<Tag> tags);
    }

    public class TagService : ITagService
    {
        private readonly ITagRepository tagRepository;

        public TagService(ITagRepository tagRepository)
        {
            this.tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(TagRepository));
        }

        public async Task<IEnumerable<Tag>> GetUserTagsAsync(Guid userID)
        {
            return await tagRepository.GetTagsByUserAsync(userID);
        }

        public async Task<bool> UpdateUserTagsAsync(IEnumerable<Tag> tags)
        {
            return await tagRepository.UpdateUserTagsAsync(tags);
        }

    }
}
