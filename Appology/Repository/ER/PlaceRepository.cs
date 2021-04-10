using Appology.Enums;
using Appology.ER.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Appology.Repository;

namespace Appology.ER.Repository
{
    public interface IPlaceRepository
    {
        Task<IEnumerable<Place>> GetAllAsync();
    }

    public class PlaceRepository : DapperBaseRepository, IPlaceRepository
    {
        private static readonly string TABLE = Tables.Name(Table.Places);

        public PlaceRepository(Func<IDbConnection> dbConnectionFactory) : base(dbConnectionFactory) { }

        public async Task<IEnumerable<Place>> GetAllAsync()
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
                ON p.ServiceId = s.Id";

            return await QueryAsync<Place>(sqlTxt);
            
        }
    }
}
