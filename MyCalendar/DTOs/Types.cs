using MyCalendar.Enums;
using MyCalendar.Model;
using System;
using System.Collections.Generic;

namespace MyCalendar.DTOs
{
    public class TypesDTO
    {
        public User User { get; set; }
        public IEnumerable<Types> UserTypes { get; set; }
        public IEnumerable<Types> SuperTypes { get; set; }
        public Guid[] Id { get; set; }
        public Guid[] InviteeId { get; set; }
        public string[] Name { get; set; }
    }
}