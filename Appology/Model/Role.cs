using Appology.Enums;
using DFM.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;

namespace Appology.Model
{
    public class Role
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool Superadmin { get; set; } = false;
    }

    public class RoleMap : EntityTypeConfiguration<Role>
    {
        public RoleMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable(Tables.Name(Table.Roles));

            // Relationships
        }
    }
}
