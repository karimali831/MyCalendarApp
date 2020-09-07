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
        public Guid EventID { get; set; }
        public Guid UserID { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public string ThemeColor { get; set; }
        public bool IsFullDay { get; set; }

        public static EventDTO MapFrom(Event e)
        {
            return new EventDTO
            {
                EventID = e.EventID,
                UserID = e.UserID,
                Subject = e.Subject,
                Description = e.Description,
                Start = e.StartDate,
                End = e.EndDate,
                ThemeColor = e.ThemeColor,
                IsFullDay = e.IsFullDay
            };
        }

        public static Event MapTo(EventDTO e)
        {
            return new Event
            {
                EventID = e.EventID,
                UserID = e.UserID,
                Subject = e.Subject,
                Description = e.Description,
                StartDate = e.Start,
                EndDate = e.End,
                ThemeColor = e.ThemeColor,
                IsFullDay = e.IsFullDay
            };
        }
    }
}
