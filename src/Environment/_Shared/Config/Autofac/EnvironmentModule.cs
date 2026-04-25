using Autofac;

namespace Reaparr.Environment;

public class EnvironmentModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<AppBuildInfo>().As<IAppBuildInfo>().SingleInstance();
        builder.Register(ctx => new PathProvider(ctx.Resolve<IAppBuildInfo>())).As<IPathProvider>().SingleInstance();
    }
}
