using DFM.Utils;
using MyCalendar.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;

namespace MyCalendar.Model
{
    public class Group
    {
        public TypeGroup Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string InviteDescription { get; set; }
        public int FeatureId { get; set; }
        public string FaIcon { get; set; }
    }

    public class GroupMap : EntityTypeConfiguration<Group>
    {
        public GroupMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("dbo.Groups");

            // Relationships
        }
    }
}
