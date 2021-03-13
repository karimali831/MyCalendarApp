using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appology.MiCalendar.Model
{

    public class EventActivityTagGroup
    {
        public string TagGroupName { get; set; }
        public int TagGroupdId { get; set; }
    }

    public class HoursWorkedInTag
    {
        public int TagGroupId { get; set; }
        public string Text { get; set; }
        public bool MultiUsers { get; set; }
        public string ActivityTag { get; set; }
        public string Color { get; set; }
        public IList<string> Avatars { get; set; }
    }
}
