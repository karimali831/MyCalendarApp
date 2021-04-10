using Appology.Enums;
using Appology.Repository;
using DFM.Utils;
using System;
using System.Collections.Generic;
using System.Data;
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

    public class BaseRepository : DapperBaseRepository, IBaseRepository
    {
        public BaseRepository(Func<IDbConnection> dbConnectionFactory) : base(dbConnectionFactory) { }

        public async Task<IList<(string, int)>> CheckDuplicates(string column, Table table)
        {
            return await QueryAsync<(string, int)>($@"
                SELECT {column}, COUNT(*) As Duplicates
                FROM {Tables.Name(table)}
                WHERE {column} is not null
                GROUP BY {column} 
                HAVING COUNT(*) > 1");
            
        }

        public async Task DeleteDuplicates(string column, Table table)
        {
            await ExecuteAsync($@"
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

        public async Task DeleteDuplicatesByLike(string like, string column, Table table)
        {
            await ExecuteAsync($"DELETE FROM {Tables.Name(table)} WHERE {column} LIKE '%{like}%'");
        }


        public async Task UpdateAsync<T>(string field, T value, int id, Table table) where T : class
        {
            await ExecuteAsync($@"
                UPDATE {Tables.Name(table)} SET {field} = @value WHERE Id = @id",
                new
                {
                    value,
                    id
                }
            );
        }

        public async Task DeleteAsync(int Id, Table table)
        {
            await ExecuteAsync($"{DapperHelper.DELETE(Tables.Name(table))} WHERE Id = @Id", new { Id });
        }
    }
}
