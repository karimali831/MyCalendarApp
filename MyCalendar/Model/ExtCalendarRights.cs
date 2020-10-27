using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCalendar.Model
{
    public class ExtCalendarRights
    {
        public string Id { get; set; }
        public bool Read { get; set; }
        public bool Save { get; set; }
        public bool Delete { get; set; }
    }
}
