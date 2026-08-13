using System.IO.Abstractions;
using Autofac;
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
        builder.RegisterType<DiskProvider>().As<IDiskProvider>().SingleInstance();

        // System.IO.Abstractions
        builder.RegisterType<System.IO.Abstractions.FileSystem>().As<IFileSystem>().SingleInstance();
        builder.Register(ctx => ctx.Resolve<IFileSystem>().Path).As<IPath>().SingleInstance();
        builder.Register(ctx => ctx.Resolve<IFileSystem>().File).As<IFile>().SingleInstance();
        builder.Register(ctx => ctx.Resolve<IFileSystem>().FileInfo).As<IFileInfoFactory>().SingleInstance();
        builder.Register(ctx => ctx.Resolve<IFileSystem>().Directory).As<IDirectory>().SingleInstance();
    }
}
