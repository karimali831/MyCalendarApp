using Ninject.Modules;
using Appology.Write.Service;
using Appology.Write.Repository;
using Appology.Service;
using Appology.MiCalendar.Service;

namespace Appology.Ninject
{
    public class WriteModule : NinjectModule
    {
        public override void Load()
        {
            // Services
            Bind<IDocumentService>().To<DocumentService>();

            // Repositories
            Bind<IDocumentRepository>().To<DocumentRepository>();
            Bind<IDocumentChangelogRepository>().To<DocumentChangelogRepository>();
        }
    }
}