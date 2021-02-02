using Appology.Enums;
using Appology.Model;
using Appology.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Appology.Service
{
    public interface ICategoryService
    {
        Task<Category> GetAsync(int Id);
        Task<IEnumerable<Category>> GetAllAsync(Categories parentId, bool activeOnly = true);
    }

    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            this.categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(CategoryRepository));
        }

        public async Task<Category> GetAsync(int Id)
        {
            return await categoryRepository.GetAsync(Id);
        }

        public async Task<IEnumerable<Category>> GetAllAsync(Categories parentId, bool activeOnly = true)
        {
            return (await categoryRepository.GetAllAsync(parentId))
                .Where(x => (activeOnly && x.Active) || !activeOnly);
        }
    }
}
