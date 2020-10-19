using MyCalendar.DTOs;
using MyCalendar.Model;
using System.Collections.Generic;

namespace MyCalendar.Website.ViewModels
{
    public class OverviewVM
    {
        public DateFilter Filter { get; set; }
        public IList<HoursWorkedInTag> HoursWorkedInTag { get; set; }
    }
}
