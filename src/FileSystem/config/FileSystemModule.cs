using Autofac;
using PlexRipper.Application.Common.Interfaces.FileSystem;

namespace PlexRipper.FileSystem.config
{
    public class FileSystemModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<FileSystem>().As<IFileSystem>().SingleInstance();
            builder.RegisterType<FileManagement>().As<IFileManagement>().SingleInstance();
            builder.RegisterType<DiskProvider>().As<IDiskProvider>().SingleInstance();
        }
    }
}
