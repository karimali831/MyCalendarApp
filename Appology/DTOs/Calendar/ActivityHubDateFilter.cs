using Appology.DTOs;

namespace Appology.MiCalendar.DTOs
{
    public class ActivityHubDateFilter : BaseDateFilter
    {
        public override string DateField { get; set; } = "Date";
        public override bool UpcomingIncEndDate { get; set; } = true; 
    }
}
