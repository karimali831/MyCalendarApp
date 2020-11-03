using MyCalendar.Enums;
using MyCalendar.Model;
using System;
using System.Collections.Generic;

namespace MyCalendar.DTOs
{
    public class TagsDTO
    {
        public Guid UserID { get; set; }
        public IEnumerable<Types> TagTypes { get; set; }
        public IEnumerable<Tag> Tags { get; set; }
        public Guid[] Id { get; set; }
        public Guid[] UserCreatedId { get; set; }
        public string[] Name { get; set; }
        public string[] ThemeColor { get; set; }
        public int[] TypeID { get; set; }
        public TagPrivacy[] Privacy { get; set; } 
    }
}