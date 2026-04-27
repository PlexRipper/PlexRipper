using System.IO.Abstractions;
using System.Reflection;
using Autofac;
using Autofac.Extras.Quartz;
using Module = Autofac.Module;

namespace Reaparr.FileSystem;

/// <summary>
/// Used to register all dependencies in Autofac for the FileSystem project.
/// </summary>
public class FileSystemModule : Module
{
    /// <inheritdoc/>
    protected override void Load(ContainerBuilder builder)
    {
        var assembly = Assembly.GetExecutingAssembly();

        builder.RegisterType<DiskProvider>().As<IDiskProvider>().SingleInstance();

        builder.RegisterModule(new QuartzAutofacJobsModule(assembly));

        // System.IO.Abstractions
        builder.RegisterType<System.IO.Abstractions.FileSystem>().As<IFileSystem>().SingleInstance();
        builder.Register(ctx => ctx.Resolve<IFileSystem>().Path).As<IPath>().SingleInstance();
        builder.Register(ctx => ctx.Resolve<IFileSystem>().File).As<IFile>().SingleInstance();
        builder.Register(ctx => ctx.Resolve<IFileSystem>().Directory).As<IDirectory>().SingleInstance();
    }
}
