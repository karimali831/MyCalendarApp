using MyCalendar.Model;
using System;
using System.Collections.Generic;

namespace MyCalendar.Website.ViewModels
{
    public abstract class BaseVM
    {
        public User User { get; set; }
        public IList<User> Users { get; set; }
        public Guid? Viewing { get; set; }
        public bool Combined { get; set; }
        public Exception Exception { get; set; }
    }
}