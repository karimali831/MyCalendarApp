using Appology.MiFinance.Enums;
using System;

namespace Appology.MiFinance.DTOs
{
    public class ReminderDTO
    {
        public string Notes { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime AddedDate => DateTime.UtcNow;
        public Priority Priority { get; set; }
        public Categories CatId { get; set; }
        public string MonzoTransId { get; set; }
    }
}
