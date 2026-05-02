using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace Reaparr.Build;

public sealed class AutofacTypeRegistrar(
    IServiceCollection services,
    Action<ContainerBuilder>? registerAutofac = null
) : ITypeRegistrar, IDisposable
{
    private readonly ContainerBuilder _builder = CreateContainerBuilder(services, registerAutofac);
    private IContainer? _container;

    public ITypeResolver Build()
    {
        _container = _builder.Build();
        return new AutofacTypeResolver(_container);
    }

    public void Register(Type service, Type implementation) => _builder.RegisterType(implementation).As(service);

    public void RegisterInstance(Type service, object implementation) =>
        _builder.RegisterInstance(implementation).As(service);

    public void RegisterLazy(Type service, Func<object> factory) =>
        _builder.Register(_ => factory()).As(service);

    public void Dispose() => _container?.Dispose();

    private static ContainerBuilder CreateContainerBuilder(
        IServiceCollection services,
        Action<ContainerBuilder>? registerAutofac
    )
    {
        var builder = new ContainerBuilder();
        builder.Populate(services);
        registerAutofac?.Invoke(builder);
        return builder;
    }
}

internal sealed class AutofacTypeResolver(IComponentContext context) : ITypeResolver
{
    public object? Resolve(Type? type) => type is null ? null : context.ResolveOptional(type);
}
