using Appology.MiCalendar.Enums;
using Appology.Enums;
using System;

namespace Appology.DTOs
{
    public abstract class BaseDateFilter
    {
        public DateFrequency? Frequency { get; set; }
        public int? Interval { get; set; }
        public abstract string DateField { get; set; }
        public abstract bool UpcomingIncEndDate { get; set; }
        public DateTime? FromDateRange { get; set; }
        public DateTime? ToDateRange { get; set; }
    }
}
