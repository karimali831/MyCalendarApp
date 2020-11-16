using MyCalendar.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyCalendar.Website.ViewModels
{
    public class CalendarVM
    {
        public IEnumerable<Types> UserCalendars { get; set; }
    }
}