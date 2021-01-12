using MyCalendar.ER.Model;
using MyCalendar.ER.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyCalendar.ER.Service
{
    public interface ITripService
    {
        Task<Trip> GetAsync(Guid custId);
        Task<(Trip Trip, bool Status)> InsertOrUpdateAsync(Trip trip);
    }

    public class TripService : ITripService
    {
        private readonly ITripRepository tripRepository;

        public TripService(ITripRepository tripRepository)
        {
            this.tripRepository = tripRepository ?? throw new ArgumentNullException(nameof(tripRepository));
        }

        public async Task<Trip> GetAsync(Guid tripId)
        {
            return await tripRepository.GetAsync(tripId);
        }

        public async Task<(Trip Trip, bool Status)> InsertOrUpdateAsync(Trip trip)
        {
            return await tripRepository.InsertOrUpdateAsync(trip);
        }
    }
}
