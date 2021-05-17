using Appology.Enums;
using Appology.MiCalendar.Enums;
using Appology.Model;
using DFM.Utils;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;

namespace Appology.MiCalendar.Model
{
    public class Tag
    {
        public Guid Id { get; set; }
        public Guid UserID { get; set; }
        public int TypeID { get; set; }
        public string Name { get; set; }
        public string ThemeColor { get; set; }
        public TimeFrequency? TargetFrequency { get; set; }
        public int? TargetValue { get; set; }
        public string TargetUnit { get; set; }
        public DayOfWeek StartDayOfWeek { get; set; }
        [DbIgnore]
        public string TypeName { get; set; }
        [DbIgnore]
        public string InviteeIds { get; set; }
        [DbIgnore]
        public bool UpdateDisabled { get; set; }
        [DbIgnore]
        public IEnumerable<Guid> InviteeIdsList => (!string.IsNullOrEmpty(InviteeIds) ? InviteeIds.Split(',').Select(x => Guid.Parse(x)) : Enumerable.Empty<Guid>());
    }

    public class TagMap : EntityTypeConfiguration<Types>
    {
        public TagMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable(Tables.Name(Table.Tags));

            // Relationships
        }
    }
}
