using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MyCalendar.DTOs
{
    public class DocumentDTO
    {
        public int TypeId { get; set; } 
        public string Title { get; set; }
        [AllowHtml]
        public string Text { get; set; }
        public Guid? Id { get; set; }
    }
}
