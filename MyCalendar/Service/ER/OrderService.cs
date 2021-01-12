using MyCalendar.ER.Model;
using MyCalendar.ER.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyCalendar.ER.Service
{
    public interface IOrderService
    {
        Task<Order> GetAsync(Guid custId);
        Task<(Order Order, bool Status)> InsertOrUpdateAsync(Order order);
    }

    public class OrderService : IOrderService
    {
        private readonly IOrderRepository orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            this.orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        }

        public async Task<Order> GetAsync(Guid orderId)
        {
            return await orderRepository.GetAsync(orderId);
        }

        public async Task<(Order Order, bool Status)> InsertOrUpdateAsync(Order order)
        {
            return await orderRepository.InsertOrUpdateAsync(order);
        }
    }
}
