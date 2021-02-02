using Appology.Enums;
using System;
using System.Data.Entity.ModelConfiguration;

namespace Appology.MiFinance.Model
{
    public class Spending
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Info { get; set; }
        public int CatId { get; set; }
        public int? SecondCatId { get; set; }
        public string Category { get; set; }
        public string SecondCategory { get; set; }
        public int? FinanceId { get; set; }
        public string MonzoTransId { get; set; }
        public bool CashExpense { get; set; }
    }

    public class SpendingMap : EntityTypeConfiguration<Spending>
    {
        public SpendingMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable(Tables.Name(Table.Spendings));
            this.Property(t => t.Id).HasColumnName("Id");

            // Relationships
        }
    }
}
