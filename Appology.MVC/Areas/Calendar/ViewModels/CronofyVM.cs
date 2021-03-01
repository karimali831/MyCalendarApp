using Appology.Model;
using Cronofy;
using System.Collections.Generic;

namespace Appology.Website.Areas.MiCalendar.ViewModels
{
    public class CronofyVM
    {
        public IEnumerable<Types> UserCalendars { get; set; }
        public IDictionary<Profile, Cronofy.Calendar[]> Profiles { get; set; } = null;
        public string CronofyCalendarAuthUrl { get; set; }
        public Cronofy.Calendar Calendar { get; set; }
        public IList<Cronofy.Event> Events { get; set; }
        public Cronofy.Event Event { get; set; }
        public EventVM EventVM { get; set; }
        public string[] SyncFromCalendarId { get; set; }
        public int[] SyncToCalendarId { get; set; }
        public string[] Read { get; set; }
        public string[] Save { get; set; }
        public string[] Delete { get; set; }
    }
}