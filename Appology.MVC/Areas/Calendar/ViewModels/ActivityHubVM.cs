using Appology.DTOs;
using Appology.Enums;
using Appology.MiCalendar.Model;
using System.Collections.Generic;

namespace Appology.Website.Areas.MiCalendar.ViewModels
{
    public class ActivityHubVM
    {
        public BaseDateFilter Filter { get; set; }
        public string PrevMonthName { get; set; }
        public string PrevSecondMonthName { get; set; }
        public string PrevMonthNameAbbrev { get; set; }
        public string PrevSecondMonthNameAbbrev { get; set; }
        public IEnumerable<Tag> UserTags { get; set; }
        public IEnumerable<ActivityHub> ActivityHub { get; set; }
        public Dictionary<ActivityTagGroup, IList<ActivityTagProgress>> Activities { get; set; }
    }
}
