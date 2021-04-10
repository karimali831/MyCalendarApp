using Appology.Enums;
using Appology.Helpers;
using Appology.MiFinance.DTOs;
using Appology.MiFinance.Enums;
using Appology.MiFinance.Model;
using Appology.MiFinance.ViewModels;
using Appology.Repository;
using DFM.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Categories = Appology.MiFinance.Enums.Categories;

namespace Appology.MiFinance.Repository
{
    public interface IFinanceRepository
    {
        Task<Finance> GetAsync(int financeId);
        Task<IEnumerable<Finance>> GetAllAsync();
        Task InsertAsync(FinanceDTO dto);
        Task UpdateNextDueDateAsync(DateTime dueDate, int Id);
        Task<IEnumerable<MonthComparisonChartVM>> GetIncomeExpenseTotalsByMonth(DateFilter filter);
        Task<IEnumerable<MonthComparisonChartVM>> GetFinanceTotalsByMonth(MonthComparisonChartRequestDTO request);
    }

    public class FinanceRepository : DapperBaseRepository, IFinanceRepository
    {
        private static readonly string TABLE = Tables.Name(Table.Finances);
        private static readonly string[] FIELDS = typeof(Finance).DapperFields();
        private static readonly string[] DTOFIELDS = typeof(FinanceDTO).DapperFields();

        public FinanceRepository(Func<IDbConnection> dbConnectionFactory) : base(dbConnectionFactory) { }

        public async Task<Finance> GetAsync(int financeId)
        {
            string sqlTxt = $"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE Id = @Id";
            return await QueryFirstOrDefaultAsync<Finance>(sqlTxt, new { Id = financeId });
        }

        public async Task<IEnumerable<Finance>> GetAllAsync()
        {
            return await QueryAsync<Finance>($"{DapperHelper.SELECT(TABLE, FIELDS)}");
        }

        public async Task UpdateNextDueDateAsync(DateTime dueDate, int Id)
        {
            await ExecuteAsync($@"
                UPDATE {TABLE} SET NextDueDate = @DueDate WHERE Id = @Id", new { DueDate = dueDate, Id }
            );
        }

        public async Task<IEnumerable<MonthComparisonChartVM>> GetIncomeExpenseTotalsByMonth(DateFilter filter)
        {
            string sqlTxt = $@"
                SELECT 
	                CONVERT(CHAR(7), Date, 120) as YearMonth, 
	                DATENAME(month, Date) AS MonthName, SUM(Amount) as 'Total', 
	                '{nameof(CategoryType.Spendings)}' as 'Type'
                FROM 
                    {Tables.Name(Table.Spendings)}
                WHERE 
                    {DateUtils.FilterDateSql(filter)}
                GROUP BY 
                    CONVERT(CHAR(7), Date, 120) , DATENAME(month, Date)

                UNION

                SELECT
	                CONVERT(CHAR(7), Date, 120), 
	                DATENAME(month, Date), 
	                SUM(Amount), '{nameof(CategoryType.Income)}'
                FROM 
                    {Tables.Name(Table.Incomes)}
                WHERE 
                    {DateUtils.FilterDateSql(filter)}
                    AND SourceId != {(int)Categories.SavingsPot}
                GROUP BY 
                    CONVERT(CHAR(7), Date, 120) , DATENAME(month, Date)

                UNION

                SELECT
	                CONVERT(CHAR(7), Date, 120), 
	                DATENAME(month, Date), 
	                SUM(Amount), '{nameof(CategoryType.Savings)}'
                FROM 
                    {Tables.Name(Table.Incomes)}
                WHERE 
                    {DateUtils.FilterDateSql(filter)}
                    AND SourceId = {(int)Categories.SavingsPot}
                GROUP BY 
                    CONVERT(CHAR(7), Date, 120) , DATENAME(month, Date)
                ORDER BY 
                    YearMonth";

            return await QueryAsync<MonthComparisonChartVM>(sqlTxt);
        }

        public async Task<IEnumerable<MonthComparisonChartVM>> GetFinanceTotalsByMonth(MonthComparisonChartRequestDTO request)
        {
            string sqlTxt = $@"
                SELECT 
	                CONVERT(CHAR(7), Date, 120) as YearMonth, 
	                DATENAME(month, Date) AS MonthName, SUM(Amount) as 'Total'
                FROM 
                    {Tables.Name(Table.Spendings)} s
                INNER JOIN 
                    {TABLE} f 
                ON 
                    f.Id = s.FinanceId
                WHERE 
                    {DateUtils.FilterDateSql(request.DateFilter)}
                GROUP BY 
                    CONVERT(CHAR(7), Date, 120) , DATENAME(month, Date)
                ORDER BY 
                    YearMonth";

            return await QueryAsync<MonthComparisonChartVM>(sqlTxt);

        }

        public async Task InsertAsync(FinanceDTO dto)
        {
            await ExecuteAsync($@"{DapperHelper.INSERT(TABLE, DTOFIELDS)}", dto);
        }
    }
}
