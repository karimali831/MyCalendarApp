using Ninject.Modules;
using Appology.MiFinance.Service;
using Appology.MiFinance.Repository;

namespace Appology.Ninject
{
    public class FinanceModule : NinjectModule
    {
        public override void Load()
        {
            // Services
            Bind<IFinanceService>().To<FinanceService>().InSingletonScope();
            Bind<ISpendingService>().To<SpendingService>();
            Bind<IBaseService>().To<BaseService>();
            Bind<IRemindersService>().To<RemindersService>();
            Bind<IIncomeService>().To<IncomeService>();
            Bind<IMonzoService>().To<MonzoService>();

            // Repositories
            Bind<IFinanceRepository>().To<FinanceRepository>();
            Bind<ISpendingRepository>().To<SpendingRepository>();
            Bind<MiFinance.Repository.ICategoryRepository>().To<MiFinance.Repository.CategoryRepository>();
            Bind<ISettingRepository>().To<SettingRepository>();
            Bind<IBaseRepository>().To<BaseRepository>();
            Bind<IIncomeRepository>().To<IncomeRepository>();
            Bind<IRemindersRepository>().To<RemindersRepository>();
            Bind<IMonzoRepository>().To<MonzoRepository>();
        }
    }
}