using Autofac;

namespace Reaparr.External;

/// <summary>
/// Used to register all dependencies in Autofac for the External project.
/// </summary>
public class ExternalModule : Module
{
    /// <inheritdoc/>
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<DashMpdCliWrapper>().As<IDashMpdCliWrapper>().InstancePerDependency();
    }
}
