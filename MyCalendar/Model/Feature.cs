using DFM.Utils;
using MyCalendar.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;

namespace MyCalendar.Model
{
    public class Feature
    {
        public Features Id { get; set; }
        public string Name { get; set; }
    }

    public class FeatureMap : EntityTypeConfiguration<Feature>
    {
        public FeatureMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("dbo.Features");

            // Relationships
        }
    }
}
