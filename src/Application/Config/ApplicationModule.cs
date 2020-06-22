using Autofac;
using PlexRipper.Application.Common.Interfaces.Application;
using PlexRipper.Application.Common.TestClass;
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
                .InstancePerDependency();

            builder.RegisterType<TestClass>().As<ITestClass>().SingleInstance();
        }
    }
}
