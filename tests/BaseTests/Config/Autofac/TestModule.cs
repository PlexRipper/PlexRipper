using Autofac;
using PlexRipper.DownloadManager;
using PlexRipper.FileSystem.Common;
using Settings.Contracts;

namespace PlexRipper.BaseTests;

/// <summary>
/// Add the default test mock modules here which can later be overridden
/// </summary>
public class TestModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<TestStreamTracker>().As<ITestStreamTracker>().SingleInstance();
        builder.RegisterType<MockDownloadFileStream>().As<IDownloadFileStream>().SingleInstance();
        builder.RegisterType<MockFileMergeStreamProvider>().As<IFileMergeStreamProvider>().SingleInstance();
        builder.RegisterType<MockFileMergeSystem>().As<IFileMergeSystem>().SingleInstance();
        builder.RegisterType<MockConfigManager>().As<IConfigManager>().SingleInstance();

        builder.RegisterType<TestLoggingClass>().SingleInstance();
    }
}