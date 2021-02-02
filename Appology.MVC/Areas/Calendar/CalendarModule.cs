using Ninject.Modules;
using Appology.MiCalendar.Service;
using Appology.MiCalendar.Repository;

namespace Appology.Ninject
{
    public class CalendarModule : NinjectModule
    {
        public override void Load()
        {
            // Services
            Bind<IEventService>().To<EventService>();
            Bind<ITagService>().To<TagService>();
            Bind<ITypeService>().To<TypeService>();
            Bind<ICronofyService>().To<CronofyService>();
            Bind<IDocumentService>().To<DocumentService>();

            // Repositories
            Bind<IEventRepository>().To<EventRepository>();
            Bind<ITagRepository>().To<TagRepository>();
            Bind<ITypeRepository>().To<TypeRepository>();
            Bind<IDocumentRepository>().To<DocumentRepository>();
        }
    }
}