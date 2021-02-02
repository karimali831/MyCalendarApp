using Appology.MiFinance.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appology.MiFinance.ViewModels
{
    public class MonthComparisonChartVM
    {
        public string YearMonth { get; set; }
        public string MonthName { get; set; }
        public decimal Total { get; set; }
        public CategoryType Type { get; set; }
        public int CatId { get; set; }
        public int? SecondCatId { get; set; }
        public string Category { get; set; }
        public string SecondCategory { get; set; }
        public int? SuperCatId1 { get; set; }
        public int? SuperCatId2 { get; set; }
        public int? FinanceSuperCatId { get; set; }
        public bool IsFinance { get; set; } = false;
        public int DaysInMonth { get; set; }
        public string Average { get; set; }
    }
}
