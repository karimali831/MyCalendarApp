using Appology.MiCalendar.Service;
using Appology.Repository;
using Appology.Service;
using Ninject.Modules;

namespace Appology.Ninject
{
    public class MVCModule : NinjectModule
    {
        public override void Load()
        {
            // Services
            Bind<IUserService>().To<UserService>();
            Bind<IFeatureRoleService>().To<FeatureRoleService>();
            Bind<ICategoryService>().To<CategoryService>();
            Bind<ITypeService>().To<TypeService>();
            Bind<INotificationService>().To<NotificationService>();

            // Repositories
            Bind<IUserRepository>().To<UserRepository>();
            Bind<IRoleRepository>().To<RoleRepository>();
            Bind<IGroupRepository>().To<GroupRepository>();
            Bind<IFeatureRepository>().To<FeatureRepository>();
            Bind<IFeatureRoleRepository>().To<FeatureRoleRepository>();
            Bind<ICategoryRepository>().To<CategoryRepository>();
            Bind<ITypeRepository>().To<TypeRepository>();
            Bind<INotificationRepository>().To<NotificationRepository>();
        }
    }
}