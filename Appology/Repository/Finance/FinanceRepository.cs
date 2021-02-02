using Appology.Enums;
using Appology.Helpers;
using Appology.MiFinance.DTOs;
using Appology.MiFinance.Enums;
using Appology.MiFinance.Model;
using Appology.MiFinance.ViewModels;
using Dapper;
using DFM.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

    public class FinanceRepository : IFinanceRepository
    {
        private readonly Func<IDbConnection> dbConnectionFactory;
        private static readonly string TABLE = Tables.Name(Table.Finances);
        private static readonly string[] FIELDS = typeof(Finance).DapperFields();
        private static readonly string[] DTOFIELDS = typeof(FinanceDTO).DapperFields();

        public FinanceRepository(Func<IDbConnection> dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }

        public async Task<Finance> GetAsync(int financeId)
        {
            string sqlTxt = $"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE Id = @Id";

            var sql = dbConnectionFactory();
            return (await sql.QueryAsync<Finance>(sqlTxt, new { Id = financeId })).FirstOrDefault();
        }

        public async Task<IEnumerable<Finance>> GetAllAsync()
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Finance>($"{DapperHelper.SELECT(TABLE, FIELDS)}")).ToArray();
            }
        }

        public async Task UpdateNextDueDateAsync(DateTime dueDate, int Id)
        {
            using (var sql = dbConnectionFactory())
            {
                await sql.ExecuteAsync($@"
                    UPDATE {TABLE} SET NextDueDate = @DueDate WHERE Id = @Id", new { DueDate = dueDate, Id }
                );
            }
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

            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<MonthComparisonChartVM>(sqlTxt)).ToArray();
            }
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

            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<MonthComparisonChartVM>(sqlTxt)).ToArray();
            }
        }

        public async Task InsertAsync(FinanceDTO dto)
        {
            using (var sql = dbConnectionFactory())
            {
                await sql.ExecuteAsync($@"{DapperHelper.INSERT(TABLE, DTOFIELDS)}", dto);
            }
        }
    }
}
