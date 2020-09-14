using MyCalendar.DTOs;
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
        Task<IEnumerable<Event>> GetAllAsync(Guid? userId = null);
        Task<bool> SaveEvent(Model.EventDTO dto);
        Task<bool> SaveEvents(IEnumerable<Model.EventDTO> dto);
        Task<bool> DeleteEvent(Guid eventId);
        Task<IEnumerable<Types>> GetTypes();
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

        public async Task<IEnumerable<Event>> GetAllAsync(Guid? userId = null)
        {
            var events = await eventRepository.GetAllAsync();

            if (userId.HasValue)
            {
                events = events.Where(x => x.UserID == userId);
            }

            foreach (var e in events)
            {
                e.StartDate.ToLocalTime().AddHours(-1);
                e.EndDate.Value.ToLocalTime().AddHours(-1);
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
    }
}
