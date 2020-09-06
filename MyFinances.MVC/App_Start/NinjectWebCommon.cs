[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(MyFinances.Website.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(MyFinances.Website.App_Start.NinjectWebCommon), "Stop")]

namespace MyFinances.Website.App_Start
{
    using DFM.Utils;
    using global::Ninject;
    using global::Ninject.Web.Common;
    using global::Ninject.Web.Common.WebHost;
    using global::Ninject.Web.WebApi;
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Web;
    using System.Web.Http;

    public static class NinjectWebCommon
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();


        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }

        /// <summary>
        /// Load your modules or register your services here
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Load<Ninject.MVCModule>();
        }

        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IDbConnection>>().ToMethod((ctx) => () => new SqlConnection(DatabaseHelper.DefaultConnectionString));

                RegisterServices(kernel);
                GlobalConfiguration.Configuration.DependencyResolver = new NinjectDependencyResolver(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }
    }
}