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
        Task<IEnumerable<Place>> GetAllAsync();
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

        public async Task<IEnumerable<Place>> GetAllAsync()
        {
            using (var sql = dbConnectionFactory())
            {
                string sqlTxt = $@";
                    SELECT 
                        p.Id, 
                        p.ServiceId, 
                        s.Name AS ServiceName, 
                        p.PlaceId, 
                        p.Name, 
                        p.Description, 
                        p.ApiProductUrl, 
                        p.ApiTimeslotsUrl, 
                        p.ImagePath, 
                        p.AllowManual, 
                        p.Active,
                        p.DisplayController,
                        p.DisplayConsumer
                    FROM {TABLE} AS p
                    LEFT JOIN Categories AS s
                    ON p.ServiceId = s.Id
                ";

                return (await sql.QueryAsync<Place>(sqlTxt)).ToArray();
            }
        }
    }
}
