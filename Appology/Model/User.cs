using DFM.Utils;
using Appology.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using Appology.MiCalendar.Model;
using Appology.MiCalendar.Enums;

namespace Appology.Model
{
    public class User
    {
        public Guid UserID { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string CronofyUid { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string ExtCalendars { get; set; }
        public bool EnableCronofy { get; set; } 
        public string RoleIds { get; set; }
        public string BuddyIds { get; set; }
        public string SelectedCalendars { get; set; }
        public string DefaultCalendarView { get; set; }
        public string DefaultNativeCalendarView { get; set; }
        public string Avatar { get; set; }
        public Guid? LastViewedDocId { get; set; }
        [DbIgnore]
        public IEnumerable<int> SelectedCalendarsList => (SelectedCalendars != null && SelectedCalendars.Any() ? SelectedCalendars.Split(',').Select(x => int.Parse(x)) : Enumerable.Empty<int>());
        [DbIgnore]
        public IEnumerable<Guid> RoleIdsList => (RoleIds != null && RoleIds.Any() ? RoleIds.Split(',').Select(x => Guid.Parse(x)) : Enumerable.Empty<Guid>());
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
            this.ToTable(Tables.Name(Table.Users));

            // Relationships
        }
    }
}
