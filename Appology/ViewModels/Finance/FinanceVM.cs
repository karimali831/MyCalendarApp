using Appology.MiFinance.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appology.MiFinance.ViewModels
{
    public class FinanceVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal AvgMonthlyAmount { get; set; }
        public DateTime? EndDate { get; set; }
        public int? MonthlyDueDate { get; set; }
        public DateTime? NextDueDate { get; set; }
        public OverrideDueDate OverrideNextDueDate { get; set; }
        public decimal? Remaining { get; set; }
        public bool ManualPayment { get; set; }
        public int? DaysUntilDue { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public bool DirectDebit { get; set; }
        public string MonzoTag { get; set; }
        public int? SuperCatId { get; set; }

    }
}
