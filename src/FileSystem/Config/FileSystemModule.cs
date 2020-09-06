using Autofac;
using PlexRipper.Application.Common;

namespace PlexRipper.FileSystem.config
{
    public class FileSystemModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<FileSystem>().As<IFileSystem>().SingleInstance();
            builder.RegisterType<FileManager>().As<IFileManager>().SingleInstance();
            builder.RegisterType<DiskProvider>().As<IDiskProvider>().SingleInstance();
        }
    }
}
