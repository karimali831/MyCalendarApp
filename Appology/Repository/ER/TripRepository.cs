using DFM.Utils;
using Appology.ER.Model;
using Appology.Helpers;
using System;
using System.Data;
using System.Threading.Tasks;
using Appology.Enums;
using Appology.Repository;

namespace Appology.ER.Repository
{
    public interface ITripRepository
    {
        Task<Trip> GetAsync(Guid TripId);
        Task<(Trip Trip, bool Status)> GetByOrderIdAsync(Guid orderId);
        Task<bool> TripExists(Guid tripId);
        Task<(Trip Trip, bool Status)> InsertOrUpdateAsync(Trip trip);
        Task<bool> DeleteTripByOrderId(Guid orderId);
    }

    public class TripRepository : DapperBaseRepository, ITripRepository
    {
        private static readonly string TABLE = Tables.Name(Table.Trips);
        private static readonly string[] FIELDS = typeof(Trip).DapperFields();

        public TripRepository(Func<IDbConnection> dbConnectionFactory) : base(dbConnectionFactory) { }

        public async Task<Trip> GetAsync(Guid tripId)
        {
            return await QueryFirstOrDefaultAsync<Trip>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE TripId = @tripId", new { tripId });
        }

        public async Task<(Trip Trip, bool Status)> GetByOrderIdAsync(Guid orderId)
        {
            var trip = await QueryFirstOrDefaultAsync<Trip>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE OrderId = @orderId", new { orderId });
            return (trip, trip != null);
        }

        public async Task<bool> DeleteTripByOrderId(Guid orderId)
        {
            return await ExecuteAsync($@"{DapperHelper.DELETE(TABLE)} WHERE OrderId = @orderId", new { orderId });
        }

        public async Task<bool> TripExists(Guid tripId)
        {
            return await ExecuteScalarAsync<bool>($"SELECT count(1) FROM {TABLE} WHERE TripId = @tripId", new { tripId });
        }

        public async Task<(Trip Trip, bool Status)> InsertOrUpdateAsync(Trip trip)
        {

            trip.Modified = DateUtils.FromTimeZoneToUtc(DateUtils.DateTime());

            if (!await TripExists(trip.TripId))
            {
                trip.TripId = Guid.NewGuid();
                await ExecuteAsync($"{DapperHelper.INSERT(TABLE, FIELDS)}", trip);
            }
            else
            {
                await ExecuteAsync($"{DapperHelper.UPDATE(TABLE, FIELDS, "")} WHERE TripId = @TripId", trip);
            }

            return (await GetAsync(trip.TripId), true);
        }
    }
}
