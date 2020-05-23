using Autofac;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Application.Services;
using System.Reflection;

namespace PlexRipper.Application.Config
{
    public class ApplicationModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // register all I*Services
            builder.RegisterAssemblyTypes(assembly)
                .Where(t => t.Name.EndsWith("Service"))
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<PlexService>().As<IPlexService>().SingleInstance();
            builder.RegisterType<AccountService>().As<IAccountService>().SingleInstance();

        }
    }
}
