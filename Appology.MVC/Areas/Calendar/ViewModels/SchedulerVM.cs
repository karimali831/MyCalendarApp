using Appology.MiCalendar.Model;
using Appology.Model;
using Appology.Website.ViewModels;
using System;
using System.Collections.Generic;

namespace Appology.Website.Areas.MiCalendar.ViewModels
{
    public class SchedulerVM : BaseVM
    {
        public int Dates { get; set; }
        public int CalendarId { get; set; }
        public IEnumerable<Tag> UserTags { get; set; }
        public IEnumerable<Types> Calendars { get; set; }
        public IEnumerable<Event> Events { get; set; }
        public IEnumerable<Tag> Tags { get; set; }
        public Guid[] TagId { get; set; }
        public DateTime[] StartDate { get; set; }
        public DateTime?[] EndDate { get; set; }
        public string[] Alarm { get; set; }
    }
}