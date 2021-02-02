using Ninject.Modules;
using Appology.ER.Service;
using Appology.ER.Repository;

namespace Appology.Ninject
{
    public class ErModule : NinjectModule
    {
        public override void Load()
        {
            // Services
            Bind<IStakeholderService>().To<StakeholderService>();
            Bind<IOrderService>().To<OrderService>();
            Bind<ITripService>().To<TripService>();

            // Repositories
            Bind<IStakeholderRepository>().To<StakeholderRepository>();
            Bind<IOrderRepository>().To<OrderRepository>();
            Bind<ITripRepository>().To<TripRepository>();
        }
    }
}