using Appology.Enums;
using Appology.MiFinance.DTOs;
using Appology.MiFinance.Enums;
using Appology.MiFinance.Model;
using Appology.MiFinance.Repository;
using DFM.ExceptionHandling;
using DFM.ExceptionHandling.Sentry;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Appology.MiFinance.Service
{
    public interface IBaseService
    {
        Task UpdateAsync<T>(string field, T value, int id, Table table) where T : class;
        Task DeleteAsync(int id, Table table);
        Task<IEnumerable<Category>> GetAllCategories(CategoryType? typeId, bool catsWithSubs);
        Task AddCategory(CategoryDTO dto);
        Task<string> GetCategoryName(int id);
        Task<int> GetSecondTypeId(int catId);
        Task<Setting> GetSettingsAsync();
        Task UpdateSettingsAsync(Setting settings);
        void ReportException(Exception exception);
        Task<IList<(string Name, int Duplicates)>> CheckDuplicates(string column, Table table);
    }

    public class BaseService : IBaseService
    {
        private readonly IBaseRepository baseRepository;
        private readonly ISettingRepository settingRepository;
        private readonly ICategoryRepository categoryRepository;
        private readonly IExceptionHandlerService exceptionHandlerService;

        public BaseService(
            IBaseRepository baseRepository, 
            ICategoryRepository categoryRepository,
            ISettingRepository settingRepository)
        {
            this.baseRepository = baseRepository ?? throw new ArgumentNullException(nameof(baseRepository));
            this.categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            this.settingRepository = settingRepository ?? throw new ArgumentNullException(nameof(settingRepository));
            this.exceptionHandlerService = new ExceptionHandlerService(ConfigurationManager.AppSettings["DFM.ExceptionHandling.Sentry.Environment"]);
        }

        public async Task<IList<(string Name, int Duplicates)>> CheckDuplicates(string column, Table table)
        {
            return await baseRepository.CheckDuplicates(column, table);
        }

        public async Task UpdateAsync<T>(string field, T value, int id, Table table) where T : class
        {
            await baseRepository.UpdateAsync(field, value, id, table);
        }

        public async Task DeleteAsync(int id, Table table)
        {
            await baseRepository.DeleteAsync(id, table);
        }

        public async Task<IEnumerable<Category>> GetAllCategories(CategoryType? typeId, bool catsWithSubs)
        {
            var cats = (await categoryRepository.GetAllAsync());

            if (typeId.HasValue)
            {
                cats = cats.Where(x => x.TypeId == typeId.Value);  
            }

            if (catsWithSubs)
            {
                cats = cats.Where(x => x.SecondTypeId != null && x.SecondTypeId != 0);
            }

            return cats.OrderBy(x => x.Name);
        }

        public async Task AddCategory(CategoryDTO dto)
        {
            await categoryRepository.AddCategory(dto);
        }

        public async Task<string> GetCategoryName(int id)
        {
            return await categoryRepository.GetCategoryName(id);
        }

        public async Task<int> GetSecondTypeId(int catId)
        {
            return await categoryRepository.GetSecondTypeId(catId);
        }

        public async Task<Setting> GetSettingsAsync()
        {
            return await settingRepository.GetAsync();
        }

        public async Task UpdateSettingsAsync(Setting settings)
        {
            await settingRepository.UpdateAsync(settings);
        }

        public void ReportException(Exception exception)
        {
            // Update to Sentry
            exceptionHandlerService.ReportException(exception).Submit();
        }
    }
}
