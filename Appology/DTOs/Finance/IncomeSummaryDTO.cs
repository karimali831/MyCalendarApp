using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appology.MiFinance.DTOs
{
    public class IncomeSummaryDTO
    {
        public string Cat1 { get; set; }
        public int CatId { get; set; }
        public int SecondCatId { get; set; }
        public string Cat2 { get; set; }
        public IEnumerable<IncomeSummaryDTO> SecondCats { get; set; }
        public int? SecondTypeId { get; set; }
        public decimal Total { get; set; }
        public string Average { get; set; }
    }
}
