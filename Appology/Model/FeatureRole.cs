using Appology.Enums;
using DFM.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;

namespace Appology.Model
{
    public class FeatureRole
    {
        public Guid Id { get; set; }
        public int FeatureId { get; set; }
        public string RoleIds { get; set; }
        public string Name { get; set; }
        public bool ReadRight { get; set; }
        public bool SaveRight { get; set; }
        public bool DeleteRight { get; set; }
        public bool FullRights { get; set; }
        [DbIgnore]
        public IEnumerable<Guid> RoleIdsList => (RoleIds != null && RoleIds.Any() ? RoleIds.Split(',').Select(x => Guid.Parse(x)) : Enumerable.Empty<Guid>());
    }

    public class FeatureRoleMap : EntityTypeConfiguration<FeatureRole>
    {
        public FeatureRoleMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable(Tables.Name(Table.FeatureRoles));

            // Relationships
        }
    }
}
