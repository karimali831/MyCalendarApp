using Appology.Enums;
using Appology.MiCalendar.Model;
using Appology.Model;
using System;
using System.Collections.Generic;


namespace Appology.Website.Areas.MiCalendar.ViewModels
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
        public IEnumerable<Types> UserTypes { get; set; }

    }
}