using System;
using System.Web.Mvc;

namespace Appology.Write.DTOs
{
    public class DocumentDTO
    {
        public Guid? Id { get; set; }
        public int TypeId { get; set; } 
        public string Title { get; set; }
        [AllowHtml]
        public string Text { get; set; }
        public string DraftText { get; set; }
        public Guid UserCreatedId { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid EditedById { get; set; }
        public bool EditedAuto { get; set; }
        public string[] TagsList { get; set; }
    }
}