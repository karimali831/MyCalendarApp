using Appology.Enums;
using Appology.Write.DTOs;
using DFM.Utils;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;

namespace Appology.Write.Model
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
        public string EditedBy { get; set; }
        [DbIgnore]
        public bool Pinned { get; set; } = false;
    }

    public class DocumentMap : EntityTypeConfiguration<Document>
    {
        public DocumentMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
           this.ToTable(Tables.Name(Table.Documents));

            // Relationships
        }
    }
}
