using System.Data.Entity.ModelConfiguration;

namespace MyCalendar.Model
{
    public class Calendar
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class CalendarMap : EntityTypeConfiguration<Calendar>
    {
        public CalendarMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Calendar");
            this.Property(t => t.Id).HasColumnName("Id");

            // Relationships

        }
    }
}
