using DFM.Utils;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using Appology.ER.Enums;
using Appology.Enums;

namespace Appology.ER.Model
{
    public class Stakeholder
    {
        public Guid Id { get; set; }
        public Stakeholders StakeholderId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Town { get; set; }
        public string Postcode { get; set; }
        public string ContactNo1 { get; set; }
        public string ContactNo2 { get; set; }
        public DateTime? Modified { get; set; }
        public decimal? ApiLat { get; set; }
        public decimal? ApiLng { get; set; }
        public string ApiFormattedAddress { get; set; }
        public string PaymentIds { get; set; }
        [DbIgnore]
        public IList<string> PaymentIdsList => !string.IsNullOrEmpty(PaymentIds) ? PaymentIds.Split(',') : new List<string>();

    }

    public class StakeholderMap : EntityTypeConfiguration<Stakeholder>
    {
        public StakeholderMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable(Tables.Name(Table.Stakeholders));

            // Relationships
        }
    }
}
