using Autofac;
using Environment;
using PlexRipper.Application;

namespace PlexRipper.FileSystem.Config
{
    /// <summary>
    /// Used to register all dependancies in Autofac for the FileSystem project.
    /// </summary>
    public class FileSystemModule : Module
    {
        /// <inheritdoc/>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<FileSystem>().As<IFileSystem>().SingleInstance();
            builder.RegisterType<System.IO.Abstractions.FileSystem>().As<System.IO.Abstractions.IFileSystem>().SingleInstance();

            builder.RegisterType<FileMerger>().As<IFileMerger>().SingleInstance();
            builder.RegisterType<PathProvider>().As<IPathProvider>().SingleInstance();
            builder.RegisterType<DiskProvider>().As<IDiskProvider>().SingleInstance();
        }
    }
}