using MyFinances.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFinances.DTOs
{
    public class FinanceDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public static FinanceDTO MapFrom(Finance Finance)
        {
            return new FinanceDTO
            {
                Id = Finance.Id,
                Name = Finance.Name
            };
        }
    }

    /*
     * usage: var dto = Finance.Select(b => FinanceDTO.MapFrom(b)).ToList();
    */
}
