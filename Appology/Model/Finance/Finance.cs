using Appology.Enums;
using Appology.MiFinance.Enums;
using System;
using System.Data.Entity.ModelConfiguration;

namespace Appology.MiFinance.Model
{
    public class Finance
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal AvgMonthlyAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? MonthlyDueDate { get; set; }
        public DateTime? NextDueDate { get; set; }
        public OverrideDueDate OverrideNextDueDate { get; set; }
        public int CatId { get; set; }
        public int? SecondCatId { get; set; }
        public decimal? Remaining { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? TotalPaid { get; set; }
        public bool ManualPayment { get; set; }
        public bool DirectDebit { get; set; }
        public string MonzoTag { get; set; }
        public int? SuperCatId { get; set; }
    }

    public class FinanceMap : EntityTypeConfiguration<Finance>
    {
        public FinanceMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable(Tables.Name(Table.Finances));
            this.Property(t => t.Id).HasColumnName("Id");

            // Relationships

        }
    }
}
