using MyCalendar.Enums;
using System;

namespace MyCalendar.DTOs
{
    public class DateFilter
    {
        public DateFrequency? Frequency { get; set; }
        public int? Interval { get; set; }
        public string DateField { get; set; } = "StartDate";
        public DateTime? FromDateRange { get; set; }
        public DateTime? ToDateRange { get; set; }
    }
}
