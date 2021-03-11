using DFM.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appology.Write.ViewModels
{
    public class DocumentTitles
    {
        public Guid Id { get; set; } 

        public string Title { get; set; }
        public DateTime EditedDate { get; set; }
        public Guid EditedById { get; set; }
        [DbIgnore]
        public string LastedEditedDuration { get; set; }
        [DbIgnore]
        public string LastedEditedByUserAvatar { get; set; }
    }
}
