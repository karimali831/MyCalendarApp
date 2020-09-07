using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyCalendar.Website.ViewModels
{
    public abstract class BaseVM
    {
        public bool Authenticated { get; set; } = false;
        public string Name { get; set; }
        public Exception Exception { get; set; }
    }
}