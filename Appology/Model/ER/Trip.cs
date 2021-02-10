using Appology.Enums;
using DFM.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Appology.ER.Model
{
    public class Trip
    {
        public Guid TripId { get; set; }
        public Guid OrderId { get; set; }
        public Guid AssignedRunnerId { get; set; }
        public string PickupId { get; set; }
        public string PickupPlace { get; set; }
        public decimal PickupLat { get; set; }
        public decimal PickupLng { get; set; }
        public string Distance { get; set; }
        public string Duration { get; set; }
        public string DropOffAddress { get; set; }
        public string DropOffPostcode { get; set; }
        [DbIgnore]
        public DateTime Created { get; set; }
        public DateTime? Modified { get; set; }
    }

    public class TripMap : EntityTypeConfiguration<Trip>
    {
        public TripMap()
        {
            // Primary Key
            this.HasKey(t => t.TripId);

            // Properties
            // Table & Column Mappings
            this.ToTable(Tables.Name(Table.Trips));

            // Relationships
        }
    }
}
