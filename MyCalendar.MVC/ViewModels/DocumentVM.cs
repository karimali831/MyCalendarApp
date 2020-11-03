using MyCalendar.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyCalendar.Website.ViewModels
{
    public class DocumentVM
    {
        public IEnumerable<Document> Documents { get; set; }
    }
}