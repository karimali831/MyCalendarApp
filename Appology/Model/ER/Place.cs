using Appology.Enums;
using DFM.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Appology.ER.Model
{
    public class Place
    {
        public Guid Id { get; set; }
        public string PlaceId { get; set; }
        public string Description { get; set; }
        public string ApiUrl { get; set; }
        public bool AllowManual { get; set; }
    }


    public class PlaceMap : EntityTypeConfiguration<Place>
    {
        public PlaceMap()
        {
            // Primary Key
            this.HasKey(t => t.PlaceId);

            // Properties
            // Table & Column Mappings
            this.ToTable(Tables.Name(Table.Places));

            // Relationships
        }
    }
}
