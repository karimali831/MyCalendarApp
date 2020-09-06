using MyCalendar.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCalendar.DTOs
{
    public class EventDTO
    {
        public int Id { get; set; }
        public string Subject { get; set; }

        public static EventDTO MapFrom(Event e)
        {
            return new EventDTO
            {
                Id = e.Id,
                Subject = e.Subject
            };
        }
    }

    /*
     * usage: var dto = Calendar.Select(b => CalendarDTO.MapFrom(b)).ToList();
    */
}
