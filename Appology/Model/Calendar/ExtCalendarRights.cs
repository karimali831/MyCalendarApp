﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appology.MiCalendar.Model
{
    public class ExtCalendarRights
    {
        public string SyncFromCalendarId { get; set; }
        public int SyncToCalendarId { get; set; }
        public bool Read { get; set; }
        public bool Save { get; set; }
        public bool Delete { get; set; }
    }
}
