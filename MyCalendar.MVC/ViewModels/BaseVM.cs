using MyCalendar.DTOs;
using MyCalendar.Enum;
using MyCalendar.Model;
using System;
using System.Collections.Generic;

namespace MyCalendar.Website.ViewModels
{
    public abstract class BaseVM
    {
        public User User { get; set; }
        public TagsDTO UserTags { get; set; }
        public IList<User> Users { get; set; }
        public MenuItem MenuItem {get; set; }
        public (Status? UpdateResponse, string UpdateMsg) UpdateStatus { get; set; }
        public Exception Exception { get; set; }
    }

    public class MenuItem
    {
        public bool Home { get; set; } 
        public Guid? Viewing { get; set; }
        public bool Combined { get; set; } 
        public bool Settings { get; set; } 
        public bool MultiAdd { get; set; } 
        public bool Overview { get; set; }
    }
}