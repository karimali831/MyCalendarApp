using DFM.Utils;
using Appology.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;

namespace Appology.Model
{
    public class Feature
    {
        public Features Id { get; set; }
        public string Name { get; set; }
        public string FaIcon { get; set; }
    }

    public class FeatureMap : EntityTypeConfiguration<Feature>
    {
        public FeatureMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable(Tables.Name(Table.Features));

            // Relationships
        }
    }
}
