using Appology.MiFinance.Model;
using System;
using System.Collections.Generic;


namespace Appology.MiFinance.ViewModels
{
    public class RemindersVM
    {
        public IEnumerable<Reminder> OverDueReminders { get; set; }
        public IEnumerable<Reminder> DueTodayReminders { get; set; }
        public IEnumerable<Reminder> UpcomingReminders { get; set; }
        public IEnumerable<Reminder> Alerts { get; set; }
    }
}
