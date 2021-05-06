using Appology.Enums;
using Appology.MiCalendar.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appology.MiCalendar.Model
{

    public class ActivityTagGroup
    {
        public string TagGroupName { get; set; }
        public int TagGroupdId { get; set; }
        public string Text { get; set; }
    }

    public class ActivityTagProgress
    {
        public int TagGroupId { get; set; }
        public string TargetUnit { get; set; }
        public double Value { get; set; }
        public string Text { get; set; }
        public bool MultiUsers { get; set; }
        public string ActivityTag { get; set; }
        public string Color { get; set; }
        public IList<string> Avatars { get; set; }
        public ProgressBar ProgressBar { get; set; }
        public double PreviousMonthTotalValue { get; set; }
        public double PreviousSecondMonthTotalValue { get; set; }
        public double ThisWeekTotalValue { get; set; }

    }

    public class ProgressBar
    {
        public TimeFrequency? TargetFrequency { get; set; }
        public int? TargetValue { get; set; }
        public string TargetUnit { get; set; }
        public double ActualValue { get; set; }
        public int ProgressBarPercentage { get; set; }
        public string ProgressBarColor { get; set; }
    }
}
