using DFM.Utils;
using System;
using System.Data.Entity.ModelConfiguration;

namespace MyCalendar.Model
{
    public class User
    {
        public Guid UserID { get; set; }
        public int Passcode { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        [DbIgnore]
        public bool Authenticated { get; set; } = false;
    }

    public class UserMap : EntityTypeConfiguration<User>
    {
        public UserMap()
        {
            // Primary Key
            this.HasKey(t => t.UserID);

            // Properties
            // Table & Column Mappings
            this.ToTable("dbo.Users");

            // Relationships
        }
    }
}
