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
    }
}
