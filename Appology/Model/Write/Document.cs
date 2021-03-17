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
        public string DraftText { get; set; }
        public string Text { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? EditedDate { get; set; }
        public Guid UserCreatedId { get; set; }
        public Guid? EditedById { get; set; }
        public bool EditedAuto { get; set; }
        public string Tags { get; set; }
        [DbIgnore]
        public string EditedDateStr => EditedDate.HasValue ? EditedDate.Value.ToString("dd/MM/yyyy HH:mm") : null;
        [DbIgnore]
        public string UserCreatedName { get; set; }
        [DbIgnore]
        public string EditedBy { get; set; }
        [DbIgnore]
        public bool Pinned { get; set; } = false;
        [DbIgnore]
        public IEnumerable<DocumentChangelog> Changelog { get; set; }
        [DbIgnore]
        public IEnumerable<string> TagsList => !string.IsNullOrEmpty(Tags) ? Tags.Split(',') : Enumerable.Empty<string>();
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
