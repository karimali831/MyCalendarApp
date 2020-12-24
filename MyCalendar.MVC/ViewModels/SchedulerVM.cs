using MyCalendar.DTOs;
using MyCalendar.Model;
using System;
using System.Collections.Generic;

namespace MyCalendar.Website.ViewModels
{
    public class SchedulerVM : BaseVM
    {
        public int Dates { get; set; }
        public int CalendarId { get; set; }
        public IEnumerable<Tag> UserTags { get; set; }
        public IEnumerable<Types> Calendars { get; set; }
        public IEnumerable<Model.EventDTO> Events { get; set; }
        public IEnumerable<Tag> Tags { get; set; }
        public Guid[] TagId { get; set; }
        public DateTime[] StartDate { get; set; }
        public DateTime?[] EndDate { get; set; }
        public string[] Alarm { get; set; }
    }
}