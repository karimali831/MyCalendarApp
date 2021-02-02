using Appology.Enums;
using System;
using System.Data.Entity.ModelConfiguration;

namespace Appology.MiFinance.Model
{
    public class Income
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Source { get; set; }
        public string SecondSource { get; set; }
        public int SourceId { get; set; }  
        public int? SecondSourceId { get; set; }  
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public int WeekNo { get; set; }
        public int AmazonWeekNo { get; set; }
        public DateTime AmazonWeekCommencing { get; set; }
        public string MonzoTransId { get; set; }
    }

    public class IncomeMap : EntityTypeConfiguration<Income>
    {
        public IncomeMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable(Tables.Name(Table.Incomes));
            this.Property(t => t.Id).HasColumnName("Id");

            // Relationships

        }
    }
}
