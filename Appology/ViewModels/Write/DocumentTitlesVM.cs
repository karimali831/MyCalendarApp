using DFM.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appology.Write.ViewModels
{
    public class DocumentTitlesVM
    {
        public Guid Id { get; set; }
        public int TypeId { get; set; }
        public string Title { get; set; }
        public DateTime EditedDate { get; set; }
        public Guid EditedById { get; set; }
        public bool EditedAuto { get; set; }
        public string Tags { get; set; }
        public Guid UserCreatedId { get; set; }
        [DbIgnore]
        public string LastedEditedDuration { get; set; }
        [DbIgnore]
        public string LastedEditedByUserAvatar { get; set; }
    }
}
