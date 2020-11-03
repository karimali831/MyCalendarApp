using MyCalendar.Enums;
using MyCalendar.Model;
using MyCalendar.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyCalendar.Service
{
    public interface ITypeService
    {
        Task<IEnumerable<Types>> GetSuperTypesAsync();
        Task<IEnumerable<Types>> GetAllByUserIdAsync(Guid userId);
        Task<Types> GetAsync(int Id);
    }

    public class TypeService : ITypeService
    {
        private readonly ITypeRepository typeRepository;

        public TypeService(ITypeRepository typeRepository)
        {
            this.typeRepository = typeRepository ?? throw new ArgumentNullException(nameof(TypeRepository));
        }

        public async Task<IEnumerable<Types>> GetSuperTypesAsync()
        {
            return await typeRepository.GetSuperTypesAsync();
        }

        public async Task<IEnumerable<Types>> GetAllByUserIdAsync(Guid userId)
        {
            return await typeRepository.GetAllByUserIdAsync(userId);
        }

        public async Task<Types> GetAsync(int Id)
        {
            return await typeRepository.GetAsync(Id);
        }
    }
}
