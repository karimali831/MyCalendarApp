using DFM.Utils;
using Appology.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;

namespace Appology.Model
{
    public class Notification
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Features FeatureId { get; set; }
        public string Text { get; set; }
        public bool HasRead { get; set; }
        [DbIgnore]
        public string Avatar { get; set; }
        [DbIgnore]
        public string Name { get; set; }
    }

    public class NotificationMap : EntityTypeConfiguration<Group>
    {
        public NotificationMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable(Tables.Name(Table.Notifications));

            // Relationships
        }
    }
}
