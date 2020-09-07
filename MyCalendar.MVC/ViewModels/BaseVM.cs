using MyCalendar.Model;
using System;


namespace MyCalendar.Website.ViewModels
{
    public abstract class BaseVM
    {
        public User User { get; set; }
        public Exception Exception { get; set; }
    }
}