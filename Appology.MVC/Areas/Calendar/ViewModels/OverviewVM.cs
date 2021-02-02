using Appology.DTOs;
using Appology.MiCalendar.Model;
using System.Collections.Generic;

namespace Appology.Website.Areas.MiCalendar.ViewModels
{
    public class OverviewVM
    {
        public BaseDateFilter Filter { get; set; }
        public IList<HoursWorkedInTag> HoursWorkedInTag { get; set; }
    }
}
