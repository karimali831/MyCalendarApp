using System;


namespace Appology.MiFinance.DTOs
{
    public class SpendingDTO
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public int? CatId { get; set; }
        public int? SecondCatId { get; set; }
        public int? FinanceId { get; set; }
        public string MonzoTransId { get; set; }
        public bool CashExpense { get; set; }
    }

    public class SpendingRequestDTO
    {
        public int? CatId { get; set; }
        public DateFilter DateFilter { get; set; }
        public bool IsFinance { get; set; } = false;
        public bool IsSecondCat { get; set; } = false;
    }
}
