using MyCalendar.ER.Model;
using MyCalendar.ER.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyCalendar.ER.Service
{
    public interface ICustomerService
    {
        Task<IEnumerable<Customer>> GetAllAsync(string filter = null);
    }

    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            this.customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
        }

        public async Task<IEnumerable<Customer>> GetAllAsync(string filter = null)
        {
            return await customerRepository.GetAllAsync(filter);
        }
    }
}
