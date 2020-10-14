using MyCalendar.DTOs;
using System.Collections.Generic;

namespace MyCalendar.Website.ViewModels
{
    public class OverviewVM : BaseVM
    {
        public DateFilter Filter { get; set; }
        public IList<HoursWorkedInTag> HoursWorkedInTag { get; set; }
    }
    public class HoursWorkedInTag
    {
        public string Text { get; set; }
        public bool MultiUsers { get; set; }
        public string Color { get; set; }
        public string TypeName { get; set; }
    }
}
