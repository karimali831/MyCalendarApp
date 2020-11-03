using DFM.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;

namespace MyCalendar.Model
{
    public class Role
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool AccessCalendar { get; set; }
        public bool AccessDocument { get; set; }
        public bool EditUsers { get; set; }
        public bool EditRoles { get; set; }
    }

    public class RoleMap : EntityTypeConfiguration<Role>
    {
        public RoleMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("dbo.Roles");

            // Relationships
        }
    }
}
