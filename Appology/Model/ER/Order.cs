﻿using Appology.Enums;
using DFM.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Appology.ER.Model
{
    public class Order
    {
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public int ServiceId { get; set; }
        public string Items { get; set; }
        public decimal OrderValue { get; set; }
        public decimal ServiceFee { get; set; }
        public decimal OrderFee { get; set; }
        public decimal DeliveryFee { get; set; }
        public int TotalItems { get; set; }
        public decimal Invoice { get; set; }
        public decimal NET { get; set; }
        public decimal DriverFee { get; set; }
        public decimal DriverEarning { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string Timeslot { get; set; }
        public bool Dispatched { get; set; }
        public bool Paid { get; set; }
        public string StripePaymentConfirmationId { get; set; }
        [DbIgnore]
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        [DbIgnore]
        public string ServiceName { get; set; }
        [DbIgnore]
        public IEnumerable<OrderItems> OrderItems { get; set; }
    }

    public class OrderItems
    {
        public string Name { get; set; }
        public int Qty { get; set; }
        public decimal Cost { get; set; }
        public string Notes { get; set; }
        public int MaxQuantity { get; set; }
        public string Image { get; set; }
    }

    public class OrderMap : EntityTypeConfiguration<Order>
    {
        public OrderMap()
        {
            // Primary Key
            this.HasKey(t => t.OrderId);

            // Properties
            // Table & Column Mappings
            this.ToTable(Tables.Name(Table.Orders));

            // Relationships
        }
    }
}
