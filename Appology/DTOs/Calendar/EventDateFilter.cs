using Appology.DTOs;

namespace Appology.MiCalendar.DTOs
{
    public class EventDateFilter : BaseDateFilter
    {
        public override string DateField { get; set; } = "StartDate";
        public override bool UpcomingIncEndDate { get; set; } = true; 
    }
}
