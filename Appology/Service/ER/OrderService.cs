using Appology.ER.Model;
using Appology.ER.Repository;
using Appology.MiCalendar.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Appology.ER.Service
{
    public interface IOrderService
    {
        Task<(Order Order, bool Status)> GetAsync(Guid orderId);
        Task<IEnumerable<Place>> GetAllPlacesAsync();
        Task<IEnumerable<Order>> GetAllAsync(Guid customerId);
        Task<(Order Order, Trip Trip, bool Status)> InsertOrUpdateAsync(Order order, Trip trip);
        Task<bool> DeleteOrder(Guid orderId);
        Task<bool> SetDeliveryDate(Guid orderId, DateTime date, string timeslot);
        Task<bool> UnsetDeliveryDate(Guid orderId);
        Task<bool> OrderPaid(Guid orderId, bool paid, string stripePaymentConfirmationId = null);
        Task<bool> OrderDispatch(Guid orderId, bool dispatch);
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

        public async Task<IEnumerable<Place>> GetAllPlacesAsync()
        {
            return
                (await placeRepository.GetAllAsync())
                    .Select(x =>
                    {
                        x.ImagePath = ERUtils.StoreImageSrc(x.ImagePath); return x;
                    });
        }

        public async Task<bool> SetDeliveryDate(Guid orderId, DateTime date, string timeslot)
        {
            return await orderRepository.SetDeliveryDate(orderId, date, timeslot);
        }

        public async Task<bool> UnsetDeliveryDate(Guid orderId)
        {
            return await orderRepository.UnsetDeliveryDate(orderId);

        }

        public async Task<bool> OrderPaid(Guid orderId, bool paid, string stripePaymentConfirmationId = null)
        {
            return await orderRepository.OrderPaid(orderId, paid, stripePaymentConfirmationId);
        }
        public async Task<bool> OrderDispatch(Guid orderId, bool dispatch)
        {
            return await orderRepository.OrderDispatch(orderId, dispatch);
        }
    }
}