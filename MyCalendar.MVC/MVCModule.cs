using Ninject.Modules;
using MyCalendar.Repository;
using MyCalendar.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyCalendar.ER.Service;
using MyCalendar.ER.Repository;

namespace MyCalendar.Ninject
{
    public class MVCModule : NinjectModule
    {
        public override void Load()
        {
            // Services
            Bind<IEventService>().To<EventService>();
            Bind<IUserService>().To<UserService>();
            Bind<ITagService>().To<TagService>();
            Bind<ITypeService>().To<TypeService>();
            Bind<ICronofyService>().To<CronofyService>();
            Bind<IDocumentService>().To<DocumentService>();
            Bind<IFeatureRoleService>().To<FeatureRoleService>();
            Bind<ICustomerService>().To<CustomerService>();

            // Repositories
            Bind<IEventRepository>().To<EventRepository>();
            Bind<IUserRepository>().To<UserRepository>();
            Bind<ITagRepository>().To<TagRepository>();
            Bind<ITypeRepository>().To<TypeRepository>();
            Bind<IDocumentRepository>().To<DocumentRepository>();
            Bind<IRoleRepository>().To<RoleRepository>();
            Bind<IGroupRepository>().To<GroupRepository>();
            Bind<IFeatureRepository>().To<FeatureRepository>();
            Bind<IFeatureRoleRepository>().To<FeatureRoleRepository>();
            Bind<ICustomerRepository>().To<CustomerRepository>();
        }
    }
}