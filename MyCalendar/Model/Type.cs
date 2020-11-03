﻿using DFM.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;

namespace MyCalendar.Model
{
    public class Types
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? SuperTypeId { get; set; }
        public Guid? UserCreatedId { get; set; }
        public string InviteeIds { get; set; }
        [DbIgnore]
        public IEnumerable<Guid> JsonInviteeIds { get; set; } = Enumerable.Empty<Guid>();
    }

    public class TypesMap : EntityTypeConfiguration<Types>
    {
        public TypesMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("dbo.Types");

            // Relationships
        }
    }
}
