using DFM.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace MyCalendar.ER.Model
{
    public class Trip
    {
        public Guid TripId { get; set; }
        public Guid OrderId { get; set; }
        public Guid AssignedRunnerId { get; set; }
        public bool CustPickupLocation { get; set; }
        public bool CustDropoffLocation { get; set; }
        public string PickupId { get; set; }
        public string PickupLatLng { get; set; }
        public string PickupAddress { get; set; }
        public decimal TripMiles { get; set; }
        public int TripMins { get; set; }
        public string DropoffId { get; set; }
        public string DropoffLatLng { get; set; }
        public string DropoffAddress { get; set; }
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
            this.ToTable("dbo.[ER.Trips]");

            // Relationships
        }
    }
}
