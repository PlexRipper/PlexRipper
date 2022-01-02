using Autofac;
using PlexRipper.Application;
using PlexRipper.DownloadManager;
using PlexRipper.FileSystem.Common;

namespace PlexRipper.BaseTests.Config
{
    public class TestModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<TestStreamTracker>().As<ITestStreamTracker>().SingleInstance();
            builder.RegisterType<TestApplicationTracker>().As<ITestApplicationTracker>().SingleInstance();
            builder.RegisterType<MockConfigManager>().As<IConfigManager>().SingleInstance();
            builder.RegisterType<MockDownloadFileStream>().As<IDownloadFileStream>().SingleInstance();
            builder.RegisterType<MockFileMergeStreamProvider>().As<IFileMergeStreamProvider>().SingleInstance();
            builder.RegisterType<MockFileMergeSystem>().As<IFileMergeSystem>().SingleInstance();
        }
    }
}