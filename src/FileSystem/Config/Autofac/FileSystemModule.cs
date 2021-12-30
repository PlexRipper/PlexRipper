using System.IO.Abstractions;
using Autofac;
using Environment;
using PlexRipper.Application;
using PlexRipper.FileSystem.Common;
using IFileSystem = PlexRipper.Application.IFileSystem;

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
            builder.RegisterType<PathSystem>().As<IPathSystem>().SingleInstance();
            builder.RegisterType<DiskSystem>().As<IDiskSystem>().SingleInstance();

            builder.RegisterType<FileMerger>().As<IFileMerger>().SingleInstance();
            builder.RegisterType<PathProvider>().As<IPathProvider>().SingleInstance();
            builder.RegisterType<DiskProvider>().As<IDiskProvider>().SingleInstance();
            builder.RegisterType<FileMergeStreamProvider>().As<IFileMergeStreamProvider>().SingleInstance();
            builder.RegisterType<FileMergeSystem>().As<IFileMergeSystem>().SingleInstance();

            // System.IO.Abstractions
            builder.RegisterType<System.IO.Abstractions.FileSystem>().As<System.IO.Abstractions.IFileSystem>().SingleInstance();
            builder.RegisterType<PathWrapper>().As<IPath>().SingleInstance();
        }
    }
}