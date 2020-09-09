using MyCalendar.Model;
using MyCalendar.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyCalendar.Service
{
    public interface ITypeService
    {
        Task<IEnumerable<Types>> GetAllAsync();
    }

    public class TypeService : ITypeService
    {
        private readonly ITypeRepository typeRepository;

        public TypeService(ITypeRepository typeRepository)
        {
            this.typeRepository = typeRepository ?? throw new ArgumentNullException(nameof(TypeRepository));
        }

        public async Task<IEnumerable<Types>> GetAllAsync()
        {
            return await typeRepository.GetAllAsync();
        }
    }
}
