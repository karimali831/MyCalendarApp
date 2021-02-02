using Appology.ER.Model;
using Appology.ER.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Appology.ER.Service
{
    public interface ITripService
    {
        Task<(Trip Trip, bool Status)> GetByOrderIdAsync(Guid orderId);
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

        public async Task<(Trip Trip, bool Status)> GetByOrderIdAsync(Guid orderId)
        {
            return await tripRepository.GetByOrderIdAsync(orderId);
        }
    }
}
