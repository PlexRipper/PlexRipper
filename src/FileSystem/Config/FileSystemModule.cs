using Autofac;
using PlexRipper.Application.Common;
using IFileSystem = PlexRipper.Application.Common.IFileSystem;

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
            builder.RegisterType<FileMerger>().As<IFileMerger>().SingleInstance();
            builder.RegisterType<DiskProvider>().As<IDiskProvider>().SingleInstance();
            builder.RegisterType<LogSystem>().As<ILogSystem>().SingleInstance();
        }
    }
}