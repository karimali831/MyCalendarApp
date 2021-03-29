using Appology.ER.Model;
using Appology.ER.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Appology.ER.Service
{
    public interface IOrderService
    {
        Task<(Order Order, bool Status)> GetAsync(Guid orderId);
        Task<Place> GetPlaceAsync(string placeId);
        Task<IEnumerable<Order>> GetAllAsync(Guid customerId);
        Task<(Order Order, Trip Trip, bool Status)> InsertOrUpdateAsync(Order order, Trip trip);
        Task<bool> DeleteOrder(Guid orderId);
    }

    public class OrderService : IOrderService
    {
        private readonly IOrderRepository orderRepository;
        private readonly IPlaceRepository placeRepository;

        public OrderService(IOrderRepository orderRepository, IPlaceRepository placeRepository)
        {
            this.orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            this.placeRepository = placeRepository ?? throw new ArgumentNullException(nameof(placeRepository));
        }

        public async Task<IEnumerable<Order>> GetAllAsync(Guid customerId)
        {
            return await orderRepository.GetAllAsync(customerId);
        }

        public async Task<(Order Order, bool Status)> GetAsync(Guid orderId)
        {
            return await orderRepository.GetAsync(orderId);
        }

        public async Task<(Order Order, Trip Trip, bool Status)> InsertOrUpdateAsync(Order order, Trip trip)
        {
            return await orderRepository.InsertOrUpdateAsync(order, trip);
        }

        public async Task<bool> DeleteOrder(Guid orderId)
        {
            return await orderRepository.DeleteOrder(orderId);
        }

        public async Task<Place> GetPlaceAsync(string placeId)
        {
            return await placeRepository.GetAsync(placeId);
        }
    }
}
