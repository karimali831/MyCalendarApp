using Dapper;
using DFM.Utils;
using MyCalendar.ER.Model;
using MyCalendar.Helpers;
using MyCalendar.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace MyCalendar.ER.Repository
{
    public interface ITripRepository
    {
        Task<Trip> GetAsync(Guid TripId);
        Task<bool> TripExists(Guid tripId);
        Task<(Trip Trip, bool Status)> InsertOrUpdateAsync(Trip trip);
    }

    public class TripRepository : ITripRepository
    {
        private readonly Func<IDbConnection> dbConnectionFactory;
        private static readonly string TABLE = "[ER.Trips]";
        private static readonly string[] FIELDS = typeof(Trip).DapperFields();

        public TripRepository(Func<IDbConnection> dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }

        public async Task<Trip> GetAsync(Guid tripId)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<Trip>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE TripId = @tripId", new { tripId })).FirstOrDefault();
            }
        }

        public async Task<bool> TripExists(Guid tripId)
        {
            using (var sql = dbConnectionFactory())
            {
                return await sql.ExecuteScalarAsync<bool>($"SELECT count(1) FROM {TABLE} WHERE TripId = @tripId", new { tripId });
            }
        }

        public async Task<(Trip Trip, bool Status)> InsertOrUpdateAsync(Trip trip)
        {
            using (var sql = dbConnectionFactory())
            {
                try
                {
                    if (!await TripExists(trip.TripId))
                    {
                        trip.TripId = Guid.NewGuid();
                        await sql.ExecuteAsync($"{DapperHelper.INSERT(TABLE, FIELDS)}", trip);
                    }
                    else
                    {
                        trip.Modified = Utils.FromTimeZoneToUtc(Utils.DateTime());
                        await sql.ExecuteAsync($"{DapperHelper.UPDATE(TABLE, FIELDS, "")} WHERE TripId = @TripId", trip);
                    }

                    return (await GetAsync(trip.TripId), true);
                }
                catch (Exception exp)
                {
                    string.IsNullOrEmpty(exp.Message);
                    return (null, false);
                }
            }
        }
    }
}
