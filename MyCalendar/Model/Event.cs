using System;
using System.Data.Entity.ModelConfiguration;

namespace MyCalendar.Model
{
    public class Event
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public string ThemeColor { get; set; }
        public bool IsFullDay { get; set; }
    }

    public class EventMap : EntityTypeConfiguration<Event>
    {
        public EventMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("dbo.Events");

            // Relationships
        }
    }
}
