using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyCalendar.Website.ViewModels
{
    public class NotificationVM
    {
        public ContentResult Content { get; set; }
        public int Count { get; set; }
    }
}