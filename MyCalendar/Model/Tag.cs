using DFM.Utils;
using MyCalendar.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;

namespace MyCalendar.Model
{
    public class Tag
    {
        public Guid Id { get; set; }
        public Guid UserID { get; set; }
        public int TypeID { get; set; }
        public string Name { get; set; }
        public string ThemeColor { get; set; }
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
            this.ToTable("dbo.Tags");

            // Relationships
        }
    }
}
