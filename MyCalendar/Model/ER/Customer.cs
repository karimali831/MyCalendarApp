using DFM.Utils;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace MyCalendar.ER.Model
{
    public class Customer
    {
        public Guid CustId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Town { get; set; }
        public string Postcode { get; set; }
        public string ContactNo1 { get; set; }
        public string ContactNo2 { get; set; }
    }

    public class CustomerMap : EntityTypeConfiguration<Customer>
    {
        public CustomerMap()
        {
            // Primary Key
            this.HasKey(t => t.CustId);

            // Properties
            // Table & Column Mappings
            this.ToTable("dbo.[ER.Customers]");

            // Relationships
        }
    }
}
