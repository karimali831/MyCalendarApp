using DFM.Utils;
using Appology.MiCalendar.Enums;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using Appology.Enums;

namespace Appology.MiCalendar.Model
{
    public class Event
    {
        public Guid EventID { get; set; }
        public int CalendarId { get; set; }
        public Guid UserID { get; set; }
        public Guid? TagID { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsFullDay { get; set; }
        public bool Tentative { get; set; }
        public string EventUid { get; set; }
        public string CalendarUid { get; set; }
        public string Alarm { get; set; }
        public string Provider { get; set; }
        [DbIgnore]
        public DateTime Created { get; set; }
        public DateTime? Modified { get; set; }
        public bool Reminder { get; set; }
        [DbIgnore]
        public string Avatar { get; set; }
        [DbIgnore]
        public string Name { get; set; }
        [DbIgnore]
        public string ThemeColor { get; set; }
        [DbIgnore]
        public string InviteeIds { get; set; }
        [DbIgnore]
        public string Subject { get; set; }
        [DbIgnore]
        public IEnumerable<Guid> InviteeIdsList => (!string.IsNullOrEmpty(InviteeIds) ? InviteeIds.Split(',').Select(x => Guid.Parse(x)) : Enumerable.Empty<Guid>());
    }

    public class EventMap : EntityTypeConfiguration<Event>
    {
        public EventMap()
        {
            // Primary Key
            this.HasKey(t => t.EventID);

            // Properties
            // Table & Column Mappings
            this.ToTable(Tables.Name(Table.Events));

            // Relationships
        }
    }
}
