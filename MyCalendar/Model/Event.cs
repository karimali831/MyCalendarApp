using DFM.Utils;
using MyCalendar.Enums;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;

namespace MyCalendar.Model
{
    public class Event
    {
        public Guid EventID { get; set; }
        public int CalendarId { get; set; }
        public Guid UserID { get; set; }
        public Guid TagID { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ThemeColor { get; set; }
        public bool IsFullDay { get; set; }
        public bool Tentative { get; set; }
        public string EventUid { get; set; }
        public string CalendarUid { get; set; }
        public string InviteeIds { get; set; }
        [DbIgnore]
        public IEnumerable<Guid> InviteeIdsList => (!string.IsNullOrEmpty(InviteeIds) ? InviteeIds.Split(',').Select(x => Guid.Parse(x)) : Enumerable.Empty<Guid>());
    }

    public class EventDTO
    {
        public Guid EventID { get; set; }
        public int CalendarId { get; set; }
        public Guid UserID { get; set; }
        public Guid? TagID { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsFullDay { get; set; } = false;
        public bool Tentative { get; set; } = false;
        public string EventUid { get; set; }
        public string CalendarUid { get; set; }
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
