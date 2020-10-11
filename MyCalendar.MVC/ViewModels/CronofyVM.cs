using Cronofy;
using MyCalendar.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyCalendar.Website.ViewModels
{
    public class CronofyVM
    {
        public IDictionary<Profile, Calendar[]> Profiles { get; set; }
        public Calendar Calendar { get; set; }
        public IList<Cronofy.Event> Events { get; set; }
        public Cronofy.Event Event { get; set; }
        public EventVM EventVM { get; set; }
    }
}