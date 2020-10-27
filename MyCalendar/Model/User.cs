using DFM.Utils;
using MyCalendar.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;

namespace MyCalendar.Model
{
    public class User
    {
        public Guid UserID { get; set; }
        public int Passcode { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string CronofyUid { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string ExtCalendars { get; set; }
        public bool EnableCronofy { get; set; } 
        [DbIgnore]
        public IEnumerable<ExtCalendarRights> ExtCalendarRights { get; set; } = Enumerable.Empty<ExtCalendarRights>();
        [DbIgnore]
        public bool Authenticated { get; set; } = false;
        [DbIgnore]
        public CronofyStatus CronofyReady { get; set; }
        [DbIgnore]
        public string CronofyReadyCalendarName { get; set; }
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
