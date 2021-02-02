using Appology.Enums;
using System;
using System.Data.Entity.ModelConfiguration;

namespace Appology.MiFinance.Model
{
    public class Setting
    {
        public int Id { get; set; }
        public decimal AvailableCredit { get; set; }
        public decimal AvailableCash { get; set; }
        public DateTime StartingDate { get; set; }
    }

    public class SettingMap : EntityTypeConfiguration<Setting>
    {
        public SettingMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable(Tables.Name(Table.Settings));
            this.Property(t => t.Id).HasColumnName("Id");

            // Relationships

        }
    }
}
