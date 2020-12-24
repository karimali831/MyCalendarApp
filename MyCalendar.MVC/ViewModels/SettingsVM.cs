using Cronofy;
using MyCalendar.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyCalendar.Website.ViewModels
{
    public class SettingsVM : BaseVM
    {
        public IEnumerable<Types> UserTypes { get; set; }
        public IEnumerable<Tag> UserTags { get; set; }
        public string CronofyCalendarAuthUrl { get; set; }
    }
}