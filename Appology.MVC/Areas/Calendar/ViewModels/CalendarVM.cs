using Appology.Model;
using System.Collections.Generic;

namespace Appology.Website.Areas.MiCalendar.ViewModels
{
    public class CalendarVM
    {
        public IEnumerable<Types> UserCalendars { get; set; }
    }
}