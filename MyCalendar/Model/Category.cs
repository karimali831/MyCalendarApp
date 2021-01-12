using System.Data.Entity.ModelConfiguration;

namespace MyCalendar.Model
{
    public class Category
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Name { get; set; }
        public bool Disabled { get; set; }
        public bool Active { get; set; }
    }

    public class CategoryMap : EntityTypeConfiguration<Feature>
    {
        public CategoryMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.ToTable("dbo.Categories");
        }
    }
}
