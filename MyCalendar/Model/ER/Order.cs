using DFM.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace MyCalendar.ER.Model
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
        public int Cost { get; set; }
        public string Notes { get; set; }
    }

    public class OrderMap : EntityTypeConfiguration<Order>
    {
        public OrderMap()
        {
            // Primary Key
            this.HasKey(t => t.OrderId);

            // Properties
            // Table & Column Mappings
            this.ToTable("dbo.[ER.Orders]");

            // Relationships
        }
    }
}
