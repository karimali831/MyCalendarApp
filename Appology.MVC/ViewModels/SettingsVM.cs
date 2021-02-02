using System.Collections.Generic;
using Appology.MiCalendar.Model;

namespace Appology.Website.ViewModels
{
    public class SettingsVM : BaseVM
    {
        public IEnumerable<Types> UserTypes { get; set; }
        public IEnumerable<Tag> UserTags { get; set; }
        public IEnumerable<string> Avatars { get; set; }
        public string CronofyCalendarAuthUrl { get; set; }
    }
}