using Appology.MiCalendar.Model;
using System;
using System.Collections.Generic;


namespace Appology.Website.Areas.MiCalendar.ViewModels
{
    public class DocumentMoveVM
    {
        public IEnumerable<Types> UserTypes { get; set; }
        public Guid UserId { get; set; }
        public (string Id, string Name) Type { get; set; }
        public bool IsDocument { get; set; }

    }
}