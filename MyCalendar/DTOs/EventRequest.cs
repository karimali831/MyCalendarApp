using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCalendar.DTOs
{
    public class RequestEventDTO
    {
        public DateFilter DateFilter { get; set; }
        public int[] CalendarIds { get; set; }
        public int[] Month { get; set; }
        public int[] Year { get; set; }
    }
}
