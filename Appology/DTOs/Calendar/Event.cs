using Appology.MiCalendar.Model;
using Appology.Helpers;
using System;

namespace Appology.MiCalendar.DTOs
{
    public class EventDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string StartStr { get; set; }
        public string EndStr { get; set; }
        public bool AllDay { get; set; }
        public bool Reminder { get; set; }
        public string TagId { get; set; }
        public string Description { get; set; }
        public bool Tentative { get; set; }
        public string EventUid { get; set; }
        public string Alarm { get; set; }
        // map props
        public int CalendarId { get; set; }
        public Guid UserID { get; set; }
        public DateTime Start => DateUtils.FromTimeZoneToUtc(DateTime.Parse(StartStr));
        public DateTime? End => !string.IsNullOrEmpty(EndStr) ? DateUtils.FromTimeZoneToUtc(DateTime.Parse(EndStr)) : (DateTime?)null;


        public static Event MapFrom(EventDTO e)
        {
            return new Event
            {
                EventID = e.Id,
                CalendarId = e.CalendarId,
                UserID = e.UserID,
                TagID = !string.IsNullOrEmpty(e.TagId) ? Guid.Parse(e.TagId) : null,
                Description = e.Reminder ? e.Title : e.Description,
                StartDate = e.Start,
                EndDate =  e.End.HasValue && !e.Reminder ? e.End : null,
                IsFullDay = e.AllDay,
                Tentative = e.Tentative,
                Alarm = e.Alarm,
                Reminder = e.Reminder
            };
        }
    }
}