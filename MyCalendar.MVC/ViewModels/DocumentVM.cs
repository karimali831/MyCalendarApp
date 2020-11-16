using MyCalendar.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyCalendar.Website.ViewModels
{
    public class DocumentVM
    {
        public Guid UserId { get; set; }
        public IEnumerable<Document> Documents { get; set; }
        public IEnumerable<Types> UserFolders { get; set; }
    }
}