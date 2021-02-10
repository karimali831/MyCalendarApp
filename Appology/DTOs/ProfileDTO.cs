using Appology.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appology.DTOs
{
    public class UserInfoDTO
    {
        public Guid UserID { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class CalendarSettingsDTO
    {
        public Guid UserId { get; set; }
        public bool EnableCronofy { get; set; }
        public string DefaultCalendarView { get; set; }
        public string DefaultNativeCalendarView { get; set; }
        public string SelectedCalendars { get; set; }
    }

    public class UserTypeDTO
    {
        public int? Id { get; set; }
        public TypeGroup GroupId { get; set; }
        public string Name { get; set; }
        public Guid UserCreatedId { get; set; }
        public string InviteeIds { get; set; }

    }
}
