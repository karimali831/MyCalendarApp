using Appology.DTOs;
using Appology.MiCalendar.Model;
using System.Collections.Generic;

namespace Appology.Website.Areas.MiCalendar.ViewModels
{
    public class EventActivityHubVM
    {
        public BaseDateFilter Filter { get; set; }
        public Dictionary<EventActivityTagGroup, IList<HoursWorkedInTag>> EventsOverview { get; set; }
    }
}
