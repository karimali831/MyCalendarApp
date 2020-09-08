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
        Task<bool> SaveEvent(Event e);
        Task<bool> DeleteEvent(Guid eventId);
    }

    public class EventService : IEventService
    {
        private readonly IEventRepository eventRepository;

        public EventService(IEventRepository eventRepository)
        {
            this.eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
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
