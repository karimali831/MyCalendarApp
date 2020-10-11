using Ninject.Modules;
using MyCalendar.Repository;
using MyCalendar.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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

            // Repositories
            Bind<IEventRepository>().To<EventRepository>();
            Bind<IUserRepository>().To<UserRepository>();
            Bind<ITagRepository>().To<TagRepository>();
            Bind<ITypeRepository>().To<TypeRepository>();
        }
    }
}