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
                        if (!await EventsByTagExist(tag))
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


        public async Task<bool> EventsByTagExist(Guid tagID)
        {
            return await eventRepository.EventsByTagExist(tagID);
        }
    }
}
