﻿using MyCalendar.DTOs;
using MyCalendar.Enums;
using MyCalendar.Model;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace MyCalendar.Website.ViewModels
{
    public class BaseVM
    {
        public User User { get; set; }
        public TagsDTO UserTags { get; set; }
        public IList<User> Buddys { get; set; }
        public MenuItem MenuItem {get; set; }
        public (Status? UpdateResponse, string UpdateMsg) UpdateStatus { get; set; }
        public Exception Exception { get; set; }
        public string AppFullName = ConfigurationManager.AppSettings["AppFullName"];
        public string AppShortName = ConfigurationManager.AppSettings["AppShortName"];
    }

    public class MenuItem
    {
        public bool Home { get; set; } 
        public Guid? Viewing { get; set; }
        public bool Combined { get; set; } 
        public bool Settings { get; set; } 
        public bool MultiAdd { get; set; } 
        public bool Cronofy { get; set; } 
        public bool Overview { get; set; }
        public bool Documents { get; set; }
        public bool None { get; set; }
    }
}