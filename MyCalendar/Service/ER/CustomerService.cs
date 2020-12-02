using MyCalendar.ER.Model;
using MyCalendar.ER.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyCalendar.ER.Service
{
    public interface ICustomerService
    {
        Task<Customer> GetAsync(Guid custId);
        Task<IEnumerable<Customer>> GetAllAsync(string filter = null);
        Task<(Customer customer, string Message)> RegisterAsync(Customer customer);
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

        public async Task<Customer> GetAsync(Guid custId)
        {
            return await customerRepository.GetAsync(custId);
        }

        public async Task<(Customer customer, string Message)> RegisterAsync(Customer customer)
        {
            string message;
            Customer newCustomer = null;

            if (await customerRepository.UserDetailsExists(nameof(Customer.Email), customer.Email))
            {
                message = "Customer email already exists";
            }
            else if (
                await customerRepository.UserDetailsExists(nameof(Customer.Address1), customer.Address1) &&
                await customerRepository.UserDetailsExists(nameof(Customer.FirstName), customer.FirstName))
            {
                message = "Matching customer name and address exists";
            }
            else if (await customerRepository.UserDetailsExists(nameof(Customer.ContactNo1), customer.ContactNo1))
            {
                message = "Matching primary contact number exists";
            }
            else
            {
                var register = await customerRepository.RegisterAsync(customer);

                if (register.Status == true)
                {
                    newCustomer = register.newCustomer;
                    message = "Registration successful";
                }
                else
                {
                    message = "An error occured";
                }
            }

            return (newCustomer, message);
        }
    }
}
