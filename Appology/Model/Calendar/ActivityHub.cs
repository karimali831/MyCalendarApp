using Appology.Enums;
using DFM.Utils;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;

namespace Appology.MiCalendar.Model
{
    public class ActivityHub
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid TagId { get; set; }
        public int Minutes { get; set; }
        public DateTime Date { get; set; }
        [DbIgnore]
        public bool IsEvent { get; set; } = false;
        [DbIgnore]
        public string Subject { get; set; }
        [DbIgnore]
        public string ThemeColor { get; set; }
        [DbIgnore]
        public int WeeklyHourlyTarget { get; set; }
        [DbIgnore]
        public string Name { get; set; }
        [DbIgnore]
        public string Avatar { get; set; }
        [DbIgnore]
        public int TagGroupId { get; set; } 
        [DbIgnore]
        public string TagGroupName { get; set; }
        [DbIgnore]
        public string InviteeIds { get; set; }
        [DbIgnore]
        public IEnumerable<Guid> InviteeIdsList => (!string.IsNullOrEmpty(InviteeIds) ? InviteeIds.Split(',').Select(x => Guid.Parse(x)) : Enumerable.Empty<Guid>());
        [DbIgnore]
        public DateTime? StartDate { get; set; }
        [DbIgnore]
        public DateTime? EndDate { get; set; }
    }

    public class ActivityHubMap : EntityTypeConfiguration<ActivityHub>
    {
        public ActivityHubMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.ToTable(Tables.Name(Table.ActivityHub));
        }
    }
}
