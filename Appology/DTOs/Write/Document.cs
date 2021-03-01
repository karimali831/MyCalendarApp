using System;
using System.Web.Mvc;

namespace Appology.Write.DTOs
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
