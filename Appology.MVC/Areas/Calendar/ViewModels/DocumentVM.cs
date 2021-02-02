using Appology.MiCalendar.Model;
using System;
using System.Collections.Generic;


namespace Appology.Website.Areas.MiCalendar.ViewModels
{
    public class DocumentVM
    {
        public Guid UserId { get; set; }
        public IEnumerable<Document> Documents { get; set; }
        public Document SelectedDocument { get; set; }
        public IEnumerable<Types> UserFolders { get; set; }
    }
}