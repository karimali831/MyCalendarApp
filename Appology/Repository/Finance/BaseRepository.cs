using Appology.Enums;
using Appology.MiFinance.Model;
using Dapper;
using DFM.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Appology.MiFinance.Repository
{
    public interface IBaseRepository
    {
        Task UpdateAsync<T>(string field, T value, int id, Table table) where T : class;
        Task DeleteAsync(int Id, Table table);
        Task<IList<(string, int)>> CheckDuplicates(string column, Table table);
        Task DeleteDuplicates(string column, Table table);
        Task DeleteDuplicatesByLike(string like, string column, Table table);
    }

    public class BaseRepository : IBaseRepository
    {
        private readonly Func<IDbConnection> dbConnectionFactory;
        private static readonly string[] FIELDS = typeof(Finance).DapperFields();

        public BaseRepository(Func<IDbConnection> dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }

        public async Task<IList<(string, int)>> CheckDuplicates(string column, Table table)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<(string, int)>($@"
                    SELECT {column}, COUNT(*) As Duplicates
                    FROM {Tables.Name(table)}
                    WHERE {column} is not null
                    GROUP BY {column} 
                    HAVING COUNT(*) > 1
                "))
                .ToList();
            }
        }

        public async Task DeleteDuplicates(string column, Table table)
        {
            using (var sql = dbConnectionFactory())
            {
                await sql.ExecuteAsync($@"
                    WITH cte AS (
                        SELECT
                            {column},
                            ROW_NUMBER() OVER(
                                PARTITION BY
                                    {column}
                                ORDER BY
                                    {column}
                            ) row_num
                         FROM
                            {Tables.Name(table)}
                        where {column} is not null
                    )
                    DELETE FROM cte
                    WHERE row_num > 1"
                );
            }
        }

        public async Task DeleteDuplicatesByLike(string like, string column, Table table)
        {
            using (var sql = dbConnectionFactory())
            {
                await sql.ExecuteAsync($"DELETE FROM {Tables.Name(table)} WHERE {column} LIKE '%{like}%'");
            }
        }


        public async Task UpdateAsync<T>(string field, T value, int id, Table table) where T : class
        {
            using (var sql = dbConnectionFactory())
            {
                await sql.ExecuteAsync($@"
                    UPDATE {Tables.Name(table)} SET {field} = @value WHERE Id = @id",
                    new
                    {
                        value,
                        id
                    }
                );
            }
        }

        public async Task DeleteAsync(int Id, Table table)
        {
            using (var sql = dbConnectionFactory())
            {
                await sql.ExecuteAsync($"{DapperHelper.DELETE(Tables.Name(table))} WHERE Id = @Id", new { Id });
            }
        }

    }
}
