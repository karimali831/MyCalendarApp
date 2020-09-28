using MyCalendar.DTOs;
using MyCalendar.Enums;
using MyCalendar.Helpers;
using MyCalendar.Model;
using MyCalendar.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCalendar.Service
{
    public interface IEventService
    {
        Task<Event> GetAsync(Guid eventId);
        Task<IEnumerable<Event>> GetAllAsync(Guid? userId = null, Guid? viewing = null, DateFilter filter = null);
        Task<bool> SaveEvent(Model.EventDTO dto);
        Task<bool> SaveEvents(IEnumerable<Model.EventDTO> dto);
        Task<bool> DeleteEvent(Guid eventId);
        Task<IEnumerable<Types>> GetTypes();
        Task<Types> GetTypeAsync(int Id);
        Task<bool> EventsByTagExist(Guid tagID);
        Task<IEnumerable<Event>> GetCurrentActivityAsync();
    }

    public class EventService : IEventService
    {
        private readonly IEventRepository eventRepository;
        private readonly ITypeService typeService;

        public EventService(IEventRepository eventRepository, ITypeService typeService)
        {
            this.eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
            this.typeService = typeService ?? throw new ArgumentNullException(nameof(typeService));
        }

        public async Task<Event> GetAsync(Guid eventId)
        {
            return await eventRepository.GetAsync(eventId);
        }

        public async Task<IEnumerable<Event>> GetCurrentActivityAsync()
        {
            return (await eventRepository.GetCurrentActivityAsync())
                .Where(x => (Utils.DateTime() >= Utils.FromUtcToTimeZone(x.StartDate.AddHours(-4)) && x.EndDate.HasValue && Utils.DateTime() < Utils.FromUtcToTimeZone(x.EndDate.Value)) ||
                        (!x.EndDate.HasValue && Utils.FromUtcToTimeZone(x.StartDate).Date == Utils.DateTime().Date));
        }

        public async Task<IEnumerable<Event>> GetAllAsync(Guid? userId = null, Guid? viewing = null, DateFilter filter = null)
        {
            var events = await eventRepository.GetAllAsync(filter);

            if (userId.HasValue)
            {
                if (viewing.HasValue)
                {
                    if (userId == viewing)
                    {
                        events = events.Where(x => x.UserID == userId || x.Privacy == TagPrivacy.Shared);
                    }

                    else
                    {
                        events = events.Where(x => (x.UserID == viewing && x.Privacy != TagPrivacy.Private) || x.Privacy == TagPrivacy.Shared);
                    }
                }
                //combined
                else
                {
                    events = events.Where(x => (x.UserID != userId && x.Privacy != TagPrivacy.Private) || x.UserID == userId || x.Privacy == TagPrivacy.Shared);
                }
            }

            return events;
        }

        public async Task<bool> SaveEvent(Model.EventDTO dto)
        {
            return await eventRepository.InsertOrUpdateAsync(dto);
        }

        public async Task<bool> SaveEvents(IEnumerable<Model.EventDTO> dto)
        {
            return await eventRepository.MultiInsertAsync(dto);
        }

        public async Task<bool> DeleteEvent(Guid eventId)
        {
            return await eventRepository.DeleteAsync(eventId);
        }

        public async Task<IEnumerable<Types>> GetTypes()
        {
            return await typeService.GetAllAsync();
        }

        public async Task<bool> EventsByTagExist(Guid tagID)
        {
            return await eventRepository.EventsByTagExist(tagID);
        }

        public async Task<Types> GetTypeAsync(int Id)
        {
            return await typeService.GetAsync(Id);
        }
    }
}
