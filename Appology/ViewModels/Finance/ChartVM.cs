using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appology.MiFinance.ViewModels
{
    public class ChartVM
    {
        public IList<ChartSummaryVM> Summary { get; set; }
        public string[] Labels { get; set; }
        public IDictionary<string, MonthComparisonChartVM[]> Data { get; set; }
    }

    public class ChartSummaryVM
    {
        public string Title { get; set; } = "Summary";
        public string AveragedDaily { get; set; }
        public string AveragedMonthly { get; set; }
        public string TotalSpent { get; set; }
    }
}
