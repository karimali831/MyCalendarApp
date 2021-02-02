using System.Collections.Generic;

namespace Appology.MiFinance.DTOs
{
    public class SpendingSummaryDTO
    {
        public string Cat1 { get; set; }
        public int CatId { get; set; }
        public int SecondCatId { get; set; }
        public bool IsFinance { get; set; }
        public string Cat2 { get; set; }
        public IEnumerable<SpendingSummaryDTO> SecondCats { get; set; }
        public int? SecondTypeId { get; set; }
        public decimal Total { get; set; }
        public string Average { get; set; }
        public int? SuperCatId1 { get; set; }
        public int? SuperCatId2 { get; set; }
        public string SuperCat1 { get; set; }
        public string SuperCat2 { get; set; }
        public bool IsSpecialCat { get; set; } = false;
    }

    public class SpecialCatsSpendingSummary
    {
        public int SuperCatId { get; set; }
        public string SuperCategory { get; set; }
        public decimal Total { get; set; }
    }
}