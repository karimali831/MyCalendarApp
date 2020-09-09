using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace MyCalendar.Model
{
    public class Types
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class TypesMap : EntityTypeConfiguration<Types>
    {
        public TypesMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("dbo.Types");

            // Relationships
        }
    }
}
