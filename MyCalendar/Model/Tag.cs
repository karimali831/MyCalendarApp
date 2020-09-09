using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace MyCalendar.Model
{
    public class Tag
    {
        public Guid Id { get; set; }
        public Guid UserID { get; set; }
        public int TypeID { get; set; }
        public string Name { get; set; }
        public string ThemeColor { get; set; }
    }

    public class TagMap : EntityTypeConfiguration<Types>
    {
        public TagMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("dbo.Tags");

            // Relationships
        }
    }
}
