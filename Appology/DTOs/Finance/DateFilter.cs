using Appology.DTOs;

namespace Appology.MiFinance.DTOs
{
    public class DateFilter : BaseDateFilter
    {
        public override string DateField { get; set; } = "Date";
        public override bool UpcomingIncEndDate { get; set; } = false;
    }
}
