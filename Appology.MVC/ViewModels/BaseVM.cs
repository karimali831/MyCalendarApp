using Appology.Enums;
using Appology.Model;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace Appology.Website.ViewModels
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
        // calendar and write
        public bool MultiAdd { get; set; } 
        public bool Cronofy { get; set; } 
        public bool ActivityHub { get; set; }
        public bool Documents { get; set; }
        // errand runner
        public bool ERNewOrder { get; set; }
        // finance
        public bool FinanceAppAddSpending { get; set; }
        public bool FinanceApp { get; set; }
        public bool FinanceSettings { get; set; }
        public bool FinanceCategories { get; set; }
        public bool Monzo { get; set; }
        public bool None { get; set; }
        // admin
        public bool Cache { get; set; }
    }
}