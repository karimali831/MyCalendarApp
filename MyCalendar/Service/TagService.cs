using MyCalendar.Model;
using MyCalendar.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCalendar.Service
{
    public interface ITagService
    {
        Task<IEnumerable<Tag>> GetUserTagsAsync(Guid userID);
        Task<Tag> GetAsync(Guid tagId);
        Task<bool> UpdateUserTagsAsync(IEnumerable<Tag> tags, Guid userID);
    }

    public class TagService : ITagService
    {
        private readonly ITagRepository tagRepository;
        private readonly IEventService eventService;

        public TagService(ITagRepository tagRepository, IEventService eventService)
        {
            this.tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(TagRepository));
            this.eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
        }

        public async Task<IEnumerable<Tag>> GetUserTagsAsync(Guid userID)
        {
            var tags = await tagRepository.GetTagsByUserAsync(userID);

            if (tags != null && tags.Any())
            {
                foreach (var tag in tags)
                {
                    tag.UpdateDisabled = tag.UserID != userID ? true : false;
                }
            }

            return tags;
        }

        public async Task<Tag> GetAsync(Guid tagId)
        {
            return await tagRepository.GetAsync(tagId);
        }

        public async Task<bool> UpdateUserTagsAsync(IEnumerable<Tag> tags, Guid userID)
        {
            if (tags.Any())
            {
                var userTags = await tagRepository.GetTagsByUserAsync(userID);
                var deletingTags = userTags.Where(x => x.UserID == userID).Select(x => x.Id).Except(tags.Select(x => x.Id));

                if (userTags.Any() && deletingTags.Any())
                {
                    foreach (var tag in deletingTags)
                    {
                        if (!await eventService.EventsByTagExist(tag))
                        {
                            await tagRepository.DeleteTagByIdAsync(tag);
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                return await tagRepository.UpdateUserTagsAsync(tags, userID);
            }
            else
            {
                await tagRepository.DeleteAllUserTagsAsync(userID);
                return true;
            }
        }

    }
}
