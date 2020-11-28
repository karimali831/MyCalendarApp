using MyCalendar.DTOs;
using MyCalendar.Model;
using System;
using System.Collections.Generic;


namespace MyCalendar.Website.ViewModels
{
    public class DocumentMoveVM
    {
        public IEnumerable<Types> UserTypes { get; set; }
        public (string Id, string Name) Type { get; set; }
        public bool IsDocument { get; set; }

    }
}