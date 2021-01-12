using MyCalendar.DTOs;
using MyCalendar.Enums;
using MyCalendar.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Mvc;

namespace MyCalendar.Website.ViewModels
{
    public class BaseVM
    {
        public User User { get; set; }
        public NotificationVM Notifications { get; set; }
        public IEnumerable<Group> AccessibleGroups { get; set; }
        public IEnumerable<Feature> AccessibleFeatures { get; set; }
        public IList<User> Buddys { get; set; }
        public MenuItem MenuItem {get; set; }
        public (Status? UpdateResponse, string UpdateMsg) UpdateStatus { get; set; }
        public Exception Exception { get; set; }
        public string AppName = ConfigurationManager.AppSettings["AppName"];
    }

    public class MenuItem
    {
        public bool Home { get; set; } 
        public bool Settings { get; set; } 
        public bool MultiAdd { get; set; } 
        public bool Cronofy { get; set; } 
        public bool Overview { get; set; }
        public bool Documents { get; set; }
        public bool ERNewOrder { get; set; }
        public bool None { get; set; }
    }
}