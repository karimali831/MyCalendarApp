using DFM.Utils;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using Appology.Enums;

namespace Appology.Model
{
    public class Types
    {
        public int Id { get; set; }
        public TypeGroup GroupId { get; set; }
        public string Name { get; set; }
        public Guid UserCreatedId { get; set; }
        public string InviteeIds { get; set; }
        public int? SuperTypeId { get; set; }
        [DbIgnore]
        public IEnumerable<Guid> InviteeIdsList => (!string.IsNullOrEmpty(InviteeIds) ? InviteeIds.Split(',').Select(x => Guid.Parse(x)) : Enumerable.Empty<Guid>());
        [DbIgnore]
        public string InviteeName { get; set; }
        [DbIgnore]
        public IEnumerable<Types> Children { get; set; }
    }

    public class TypesMap : EntityTypeConfiguration<Types>
    {
        public TypesMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable(Tables.Name(Table.Types));

            // Relationships
        }
    }
}
