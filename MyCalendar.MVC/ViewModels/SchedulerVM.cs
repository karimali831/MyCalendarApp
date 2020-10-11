using MyCalendar.DTOs;
using MyCalendar.Model;
using System;
using System.Collections.Generic;

namespace MyCalendar.Website.ViewModels
{
    public class SchedulerVM : BaseVM
    {
        public Guid? TagID { get; set; }
        public int Dates { get; set; }
        public IEnumerable<Model.EventDTO> Events { get; set; }
        public IEnumerable<Tag> Tags { get; set; }
        public DateTime[] StartDate { get; set; }
        public DateTime?[] EndDate { get; set; }
        public bool Icloud { get; set; }
    }
}