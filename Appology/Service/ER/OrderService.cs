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
        Task<IEnumerable<Order>> GetAllAsync(Guid customerId);
        Task<(Order Order, Trip Trip, bool Status)> InsertOrUpdateAsync(Order order, Trip trip);
    }

    public class OrderService : IOrderService
    {
        private readonly IOrderRepository orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            this.orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
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
    }
}
