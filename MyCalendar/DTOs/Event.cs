using MyCalendar.Model;
using MyFinances.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCalendar.DTOs
{
    public class EventVM
    {
        public Guid EventID { get; set; }
        public Guid UserID { get; set; }
        public Guid TagID { get; set; }
        public string Description { get; set; }
        public string Duration { get; set; }
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public bool IsFullDay { get; set; }
        public bool Tentative { get; set; }
        public string ThemeColor { get; set; }
        public string Subject { get; set; }
    }

    public class EventDTO
    {
        public Guid EventID { get; set; }
        public Guid UserID { get; set; }
        public Guid TagID { get; set; }
        public string Description { get; set; }
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public bool IsFullDay { get; set; }
        public bool Tentative { get; set; }

        public static EventVM MapFrom(Event e)
        {
            return new EventVM
            {
                EventID = e.EventID,
                UserID = e.UserID,
                TagID = e.TagID,
                Description = e.Description,
                Start = e.StartDate.ToLocalTime(),
                End = e.EndDate.Value.ToLocalTime(),
                IsFullDay = e.IsFullDay,
                Tentative = e.Tentative,
                Subject = e.Subject,
                ThemeColor = e.ThemeColor,
                Duration = e.EndDate.HasValue ? Utils.Duration(e.EndDate.Value.ToLocalTime(), e.StartDate.ToLocalTime()) : string.Empty
            };
        }

        public static Model.EventDTO MapFrom(EventVM e)
        {
            return new Model.EventDTO
            {
                EventID = e.EventID,
                UserID = e.UserID,
                TagID = e.TagID,
                Description = e.Description,
                StartDate = e.Start.ToLocalTime(),
                EndDate = e.End.Value.ToLocalTime(),
                IsFullDay = e.IsFullDay,
                Tentative = e.Tentative
            };
        }
    }
}
