using System.IO.Abstractions;
using System.Reflection;
using Autofac;
using Autofac.Extras.Quartz;
using Environment;
using FileSystem.Contracts;
using MediatR.Extensions.Autofac.DependencyInjection;
using MediatR.Extensions.Autofac.DependencyInjection.Builder;
using Module = Autofac.Module;

namespace PlexRipper.FileSystem.Config;

/// <summary>
/// Used to register all dependencies in Autofac for the FileSystem project.
/// </summary>
public class FileSystemModule : Module
{
    /// <inheritdoc/>
    protected override void Load(ContainerBuilder builder)
    {
        var assembly = Assembly.GetExecutingAssembly();

        builder.RegisterType<FileResultSystem>().As<IFileResultSystem>().SingleInstance();
        builder.RegisterType<PathSystem>().As<IPathSystem>().SingleInstance();
        builder.RegisterType<DirectorySystem>().As<IDirectorySystem>().SingleInstance();

        builder.RegisterType<PathProvider>().As<IPathProvider>().SingleInstance();
        builder.RegisterType<DiskProvider>().As<IDiskProvider>().SingleInstance();
        builder.RegisterType<DownloadFileStream>().As<IDownloadFileStream>().SingleInstance();

        builder.RegisterModule(new QuartzAutofacJobsModule(assembly));

        // System.IO.Abstractions
        builder.RegisterType<System.IO.Abstractions.FileSystem>().As<IFileSystem>().SingleInstance();
        builder.RegisterType<PathWrapper>().As<IPath>().SingleInstance();

        // MediatR
        var configuration = MediatRConfigurationBuilder
            .Create(assembly)
            .WithAllOpenGenericHandlerTypesRegistered()
            .Build();
        builder.RegisterMediatR(configuration);
    }
}
