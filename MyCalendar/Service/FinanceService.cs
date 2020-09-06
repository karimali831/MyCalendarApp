using MyCalendar.Model;
using MyCalendar.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyCalendar.Service
{
    public interface ICalendarService
    {
        Task<IEnumerable<Calendar>> GetAllAsync();
    }

    public class CalendarService : ICalendarService
    {
        private readonly ICalendarRepository CalendarRepository;

        public CalendarService(ICalendarRepository CalendarRepository)
        {
            this.CalendarRepository = CalendarRepository ?? throw new ArgumentNullException(nameof(CalendarRepository));
        }

        public async Task<IEnumerable<Calendar>> GetAllAsync()
        {
            return await CalendarRepository.GetAllAsync();
        }
    }
}
