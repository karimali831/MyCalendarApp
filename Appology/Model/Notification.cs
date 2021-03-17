using DFM.Utils;
using Appology.Enums;
using System;
using System.Data.Entity.ModelConfiguration;

namespace Appology.Model
{
    public class Notification
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public NotificationType TypeId { get; set; }
        public string Text { get; set; }
        public bool HasRead { get; set; }
        [DbIgnore]
        public string Avatar { get; set; }
        [DbIgnore]
        public string Name { get; set; }
        [DbIgnore]
        public string FaIcon { get; set; }
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
