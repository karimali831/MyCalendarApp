﻿using Cronofy;
using MyCalendar.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyCalendar.Website.ViewModels
{
    public class SettingsVM : BaseVM
    {
        public IEnumerable<Types> Types { get; set; }
        public string CronofyCalendarAuthUrl { get; set; }
    }
}