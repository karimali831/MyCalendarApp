using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appology.MiFinance.DTOs
{
    public class FinanceDTO
    {
        public string Name { get; set; }
        public int CatId { get; set; }
        public int? SecondCatId { get; set; }
    }
}
