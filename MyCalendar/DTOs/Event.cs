using MyCalendar.Helpers;
using MyCalendar.Model;
using System;

namespace MyCalendar.DTOs
{
    public class EventVM
    {
        public Guid EventID { get; set; }
        public Guid UserID { get; set; }
        public Guid? TagID { get; set; }
        public string Description { get; set; }
        public string Duration { get; set; }
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public bool IsFullDay { get; set; }
        public bool Tentative { get; set; }
        public string ThemeColor { get; set; }
        public string Subject { get; set; }
        public string SplitDates { get; set; }
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
                Start = Utils.FromUtcToLocalTime(e.StartDate),
                End = e.EndDate.HasValue ? Utils.FromUtcToLocalTime(e.EndDate.Value) : (DateTime?)null,
                IsFullDay = e.IsFullDay,
                Tentative = e.Tentative,
                Subject = e.Subject,
                ThemeColor = e.ThemeColor,
                Duration = e.EndDate.HasValue ? Utils.Duration(e.EndDate.Value, e.StartDate) : string.Empty
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
                StartDate = Utils.FromTimeZoneToUtc(e.Start),
                EndDate =  e.End.HasValue ? Utils.FromTimeZoneToUtc(e.End.Value) : (DateTime?)null,
                IsFullDay = e.IsFullDay,
                Tentative = e.Tentative
            };
        }
    }
}
