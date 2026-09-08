namespace Reaparr.BaseTests;

public static class GlobalTestHooks
{
    [Before(TestSession)]
    public static void SetupTestSession()
    {
        var appRuntimeInfo = new MockAppRuntimeInfo { IsUnmasked = true };
        var testLogConfig = new TestLogConfig(
            appRuntimeInfo,
            new MockPathProvider(MockDatabase.GetMemoryDatabaseName())
        );

        LogFactory.SetupLogging(testLogConfig, appRuntimeInfo, LogEventLevel.Verbose);
        BogusExtensions.Setup();
    }

    [After(TestSession)]
    public static void CleanupTestSession()
    {
        LogFactory.CloseAndFlush();
    }
}
