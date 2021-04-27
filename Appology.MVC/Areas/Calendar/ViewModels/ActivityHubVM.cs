using Appology.DTOs;
using Appology.MiCalendar.Model;
using System.Collections.Generic;

namespace Appology.Website.Areas.MiCalendar.ViewModels
{
    public class ActivityHubVM
    {
        public BaseDateFilter Filter { get; set; }
        public IEnumerable<Tag> UserTags { get; set; }
        public Dictionary<ActivityTagGroup, IList<HoursWorkedInTag>> Activities { get; set; }
    }
}
