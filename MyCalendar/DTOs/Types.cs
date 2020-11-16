using MyCalendar.Enums;
using MyCalendar.Model;
using System;
using System.Collections.Generic;

namespace MyCalendar.DTOs
{
    public class TypesDTO
    {
        public User User { get; set; }
        public IEnumerable<User> Buddys { get; set; }
        public IEnumerable<Types> UserTypes { get; set; }
        public IEnumerable<Group> AccessibleGroups { get; set; }
        public int[] Id { get; set; }
        public TypeGroup[] GroupId { get; set; }
        public string[] InviteeId { get; set; }
        public string[] Name { get; set; }
    }

    public class TypeDTO
    {
        public string Name { get; set; }
        public TypeGroup GroupId { get; set; }
        public int? SuperTypeId { get; set; }
        public Guid UserCreatedId { get; set; }
        public string InviteeIds { get; set; }
    }
}