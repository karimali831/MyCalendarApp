using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appology.MiFinance.DTOs
{
    public class IncomeDTO
    {
        public string Name { get; set; }
        public int SourceId { get; set; }
        public int? SecondSourceId { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string MonzoTransId { get; set; }
    }

    public class IncomeRequestDTO
    {
        public int? SourceId { get; set; }
        public DateFilter DateFilter { get; set; }
        public bool IsSecondCat { get; set; } = false;
    }
}
