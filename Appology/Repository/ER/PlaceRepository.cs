using Dapper;
using DFM.Utils;
using Appology.Enums;
using Appology.ER.Model;
using Appology.Helpers;
using Appology.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Appology.ER.Enums;

namespace Appology.ER.Repository
{
    public interface IPlaceRepository
    {
        Task<Place> GetAsync(string placeId);
    }

    public class PlaceRepository : IPlaceRepository
    {
        private readonly Func<IDbConnection> dbConnectionFactory;
        private static readonly string TABLE = Tables.Name(Table.Places);
        private static readonly string[] FIELDS = typeof(Place).DapperFields();

        public PlaceRepository(Func<IDbConnection> dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }

        public async Task<Place> GetAsync(string placeId)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Place>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE PlaceId = @placeId", new { placeId })).FirstOrDefault();
            }
        }
    }
}
