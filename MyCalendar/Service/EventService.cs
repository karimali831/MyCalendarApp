using MyCalendar.Model;
using MyCalendar.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyCalendar.Service
{
    public interface IEventService
    {
        Task<IEnumerable<Event>> GetAllAsync();
        Task<bool> SaveEvent(Event e);
        Task<bool> DeleteEvent(Guid eventId);
    }

    public class EventService : IEventService
    {
        private readonly IEventRepository eventRepository;

        public EventService(IEventRepository calendarRepository)
        {
            this.eventRepository = calendarRepository ?? throw new ArgumentNullException(nameof(eventRepository));
        }

        public async Task<IEnumerable<Event>> GetAllAsync()
        {
            return await eventRepository.GetAllAsync();
        }

        public async Task<bool> SaveEvent(Event e)
        {
            return await eventRepository.InsertOrUpdateAsync(e);
        }

        public async Task<bool> DeleteEvent(Guid eventId)
        {
            return await eventRepository.DeleteAsync(eventId);
        }
    }
}
