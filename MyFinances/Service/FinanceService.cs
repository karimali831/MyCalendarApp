using MyFinances.Model;
using MyFinances.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyFinances.Service
{
    public interface IFinanceService
    {
        Task<IEnumerable<Finance>> GetAllAsync();
    }

    public class FinanceService : IFinanceService
    {
        private readonly IFinanceRepository FinanceRepository;

        public FinanceService(IFinanceRepository FinanceRepository)
        {
            this.FinanceRepository = FinanceRepository ?? throw new ArgumentNullException(nameof(FinanceRepository));
        }

        public async Task<IEnumerable<Finance>> GetAllAsync()
        {
            return await FinanceRepository.GetAllAsync();
        }
    }
}
