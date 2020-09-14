using DFM.Utils;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace MyCalendar.Model
{
    public class Event
    {
        public Guid EventID { get; set; }
        public Guid UserID { get; set; }
        public Guid TagID { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ThemeColor { get; set; }
        public bool IsFullDay { get; set; }
        public bool Tentative { get; set; }
    }

    public class EventDTO
    {
        public Guid EventID { get; set; }
        public Guid UserID { get; set; }
        public Guid TagID { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsFullDay { get; set; } = false;
        public bool Tentative { get; set; } = false;
    }

    public class EventMap : EntityTypeConfiguration<Event>
    {
        public EventMap()
        {
            // Primary Key
            this.HasKey(t => t.EventID);

            // Properties
            // Table & Column Mappings
            this.ToTable("dbo.Events");

            // Relationships
        }
    }
}
