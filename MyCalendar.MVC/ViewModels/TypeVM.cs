using MyCalendar.Enums;
using MyCalendar.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyCalendar.Website.ViewModels
{
    public class TypeVM
    {
        public int Id { get; set; }
        public Guid UserCreatedId { get; set; }
        public string Name { get; set; }
        public string[] InviteeId { get; set; }
        public int? SuperTypeId { get; set; }
        public TypeGroup GroupId { get; set; }
        //
        public IEnumerable<User> Buddys { get; set; }
        public bool Edit { get; set; }
        public User User { get; set; }
        public Types Type { get; set; }
        public Group Group { get; set; }

    }
}