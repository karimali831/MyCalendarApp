using Ninject.Modules;
using Appology.Write.Service;
using Appology.Write.Repository;

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
        }
    }
}