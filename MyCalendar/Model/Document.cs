using DFM.Utils;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace MyCalendar.Model
{
    public class Document
    {
        public Guid Id { get; set; }
        public int TypeId { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? EditedDate { get; set; }
        public Guid UserCreatedId { get; set; }
        public Guid? EditedById { get; set; }
        [DbIgnore]
        public string UserCreatedName { get; set; }
        [DbIgnore]
        public string EditedByName { get; set; }
    }

    public class DocumentMap : EntityTypeConfiguration<Document>
    {
        public DocumentMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("dbo.Documents");

            // Relationships
        }
    }
}
