using MyCalendar.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCalendar.DTOs
{
    public class CalendarDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public static CalendarDTO MapFrom(Calendar Calendar)
        {
            return new CalendarDTO
            {
                Id = Calendar.Id,
                Name = Calendar.Name
            };
        }
    }

    /*
     * usage: var dto = Calendar.Select(b => CalendarDTO.MapFrom(b)).ToList();
    */
}
