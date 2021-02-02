using Appology.DTOs;

namespace Appology.MiCalendar.DTOs
{
    public class RequestEventDTO
    {
        public BaseDateFilter DateFilter { get; set; }
        public int[] CalendarIds { get; set; }
        public int[] Month { get; set; }
        public int[] Year { get; set; }
    }
}
