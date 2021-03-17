using Appology.Enums;
using DFM.Utils;
using System;
using System.Data.Entity.ModelConfiguration;

namespace Appology.Write.Model
{
    public class DocumentChangelog
    {
        [DbIgnore]
        public int Id { get; set; }
        public Guid DocId { get; set; }
        public Guid UserId { get; set; }
        public string OldText { get; set; }
        public string NewText { get; set; }
        public DateTime Date { get; set; }
        [DbIgnore]
        public string EditedBy { get; set; }
        [DbIgnore]
        public string EditedDate { get; set; }
        [DbIgnore]
        public string EditedByAvatar { get; set; }
    }

    public class DocumentChangelogMap : EntityTypeConfiguration<Document>
    {
        public DocumentChangelogMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
           this.ToTable(Tables.Name(Table.DocumentChangelog));

            // Relationships
        }
    }
}
