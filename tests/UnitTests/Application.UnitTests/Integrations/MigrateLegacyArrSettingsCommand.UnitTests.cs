namespace Reaparr.Application.UnitTests;

public class MigrateLegacyArrSettingsCommandUnitTests : BaseCommandUnitTest<MigrateLegacyArrSettingsCommand>
{
    [Test]
    public async Task ShouldImportBothLegacyIntegrationsAndClearSettings_WhenValuesAreValid()
    {
        // Arrange
        var settings = IntegrationsSettings.Create();
        settings.Radarr.RadarrBaseUrl = " https://radarr.example.com/ ";
        settings.Radarr.RadarrApiKey = " radarr-key ";
        settings.Sonarr.SonarrBaseUrl = " https://sonarr.example.com/ ";
        settings.Sonarr.SonarrApiKey = " sonarr-key ";
        SetupDependencies(builder => builder.RegisterInstance(settings).As<IIntegrationsSettings>().SingleInstance());
        await SetupDatabase(55370);

        Mock.Mock<IConfigManager>().Setup(x => x.SaveConfig()).Returns(Result.Ok()).Verifiable(Times.Once());
        Mock.Mock<ICommandExecutor>()
            .Setup(x =>
                x.Send(
                    It.Is<SetupRadarrIntegrationCommand>(command => command.IntegrationId != Guid.Empty),
                    CancellationToken.None
                )
            )
            .ReturnsAsync(
                (SetupRadarrIntegrationCommand command, CancellationToken _) =>
                    Result.Ok(CreateRadarr(command.IntegrationId))
            )
            .Verifiable(Times.Once());
        Mock.Mock<ICommandExecutor>()
            .Setup(x =>
                x.Send(
                    It.Is<SetupSonarrIntegrationCommand>(command => command.IntegrationId != Guid.Empty),
                    CancellationToken.None
                )
            )
            .ReturnsAsync(
                (SetupSonarrIntegrationCommand command, CancellationToken _) =>
                    Result.Ok(CreateSonarr(command.IntegrationId))
            )
            .Verifiable(Times.Once());

        // Act
        var result = await TestHandlerExecuteAsync(new MigrateLegacyArrSettingsCommand());
        var radarr = await IDbContext.RadarrIntegrations.SingleAsync(CancellationToken);
        var sonarr = await IDbContext.SonarrIntegrations.SingleAsync(CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        radarr.DisplayName.ShouldBe("Radarr");
        radarr.BaseUrl.ShouldBe("https://radarr.example.com");
        radarr.RadarrApiKey.ShouldBe("radarr-key");
        radarr.Category.ShouldBe("reaparr-radarr");
        radarr.DownloadFolderId.ShouldBe(FolderTypeDefaults.DefaultDownloadFolderId);
        radarr.ProvisioningState.ShouldBe(IntegrationProvisioningState.Unconfigured);
        radarr.QBittorrentApiKey.ShouldStartWith("qbt_");
        radarr.QBittorrentApiKey.Length.ShouldBe(32);
        radarr.TorznabApiKey.Length.ShouldBe(32);
        sonarr.DisplayName.ShouldBe("Sonarr");
        sonarr.BaseUrl.ShouldBe("https://sonarr.example.com");
        sonarr.SonarrApiKey.ShouldBe("sonarr-key");
        sonarr.Category.ShouldBe("reaparr-sonarr");
        sonarr.DownloadFolderId.ShouldBe(FolderTypeDefaults.DefaultDownloadFolderId);
        sonarr.ProvisioningState.ShouldBe(IntegrationProvisioningState.Unconfigured);
        sonarr.QBittorrentApiKey.ShouldStartWith("qbt_");
        sonarr.QBittorrentApiKey.Length.ShouldBe(32);
        sonarr.TorznabApiKey.Length.ShouldBe(32);
        settings.Radarr.RadarrApiKey.ShouldBeEmpty();
        settings.Sonarr.SonarrApiKey.ShouldBeEmpty();
        Mock.Mock<IConfigManager>().Verify();
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldSkipInvalidEntryAndClearSettings_WhenApiKeyIsPresent()
    {
        // Arrange
        var settings = IntegrationsSettings.Create();
        settings.Radarr.RadarrBaseUrl = "not-a-url";
        settings.Radarr.RadarrApiKey = "radarr-key";
        SetupDependencies(builder => builder.RegisterInstance(settings).As<IIntegrationsSettings>().SingleInstance());
        await SetupDatabase(55371);

        Mock.Mock<IConfigManager>().Setup(x => x.SaveConfig()).Returns(Result.Ok()).Verifiable(Times.Once());
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<SetupRadarrIntegrationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<RadarrIntegration>("Setup should not run"))
            .Verifiable(Times.Never());
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<SetupSonarrIntegrationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<SonarrIntegration>("Setup should not run"))
            .Verifiable(Times.Never());

        // Act
        var result = await TestHandlerExecuteAsync(new MigrateLegacyArrSettingsCommand());

        // Assert
        result.IsSuccess.ShouldBeTrue();
        (await IDbContext.RadarrIntegrations.CountAsync(CancellationToken)).ShouldBe(0);
        (await IDbContext.SonarrIntegrations.CountAsync(CancellationToken)).ShouldBe(0);
        settings.Radarr.RadarrApiKey.ShouldBeEmpty();
        Mock.Mock<IConfigManager>().Verify();
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldRollbackAllRowsAndPreserveSettings_WhenTransactionFails()
    {
        // Arrange
        var settings = IntegrationsSettings.Create();
        var dbContext = MockIDbContext;
        settings.Radarr.RadarrBaseUrl = "https://radarr.example.com";
        settings.Radarr.RadarrApiKey = "radarr-key";
        settings.Sonarr.SonarrBaseUrl = "https://sonarr.example.com";
        settings.Sonarr.SonarrApiKey = "sonarr-key";
        SetupDependencies(builder =>
        {
            builder.RegisterInstance(settings).As<IIntegrationsSettings>().SingleInstance();
            builder.RegisterInstance(dbContext.Object).As<IReaparrDbContext>().SingleInstance();
        });

        dbContext
            .Setup(x =>
                x.ExecuteTransactionAsync(
                    It.IsAny<Func<IReaparrDbContext, CancellationToken, Task<List<IntegrationIdentity>>>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Fail<List<IntegrationIdentity>>("Transaction failed"))
            .Verifiable(Times.Once());
        Mock.Mock<IConfigManager>().Setup(x => x.SaveConfig()).Returns(Result.Ok()).Verifiable(Times.Never());
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<SetupRadarrIntegrationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<RadarrIntegration>("Setup should not run"))
            .Verifiable(Times.Never());
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<SetupSonarrIntegrationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<SonarrIntegration>("Setup should not run"))
            .Verifiable(Times.Never());

        // Act
        var result = await TestHandlerExecuteAsync(new MigrateLegacyArrSettingsCommand());

        // Assert
        result.IsFailed.ShouldBeTrue();
        settings.Radarr.RadarrApiKey.ShouldBe("radarr-key");
        settings.Sonarr.SonarrApiKey.ShouldBe("sonarr-key");
        dbContext.Verify();
        Mock.Mock<IConfigManager>().Verify();
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldAttemptEachSetupOnceAndNotRepeatMigration_WhenStartedTwice()
    {
        // Arrange
        var settings = IntegrationsSettings.Create();
        settings.Radarr.RadarrBaseUrl = "https://radarr.example.com";
        settings.Radarr.RadarrApiKey = "radarr-key";
        settings.Sonarr.SonarrBaseUrl = "https://sonarr.example.com";
        settings.Sonarr.SonarrApiKey = "sonarr-key";
        SetupDependencies(builder => builder.RegisterInstance(settings).As<IIntegrationsSettings>().SingleInstance());
        await SetupDatabase(55373);

        Mock.Mock<IConfigManager>().Setup(x => x.SaveConfig()).Returns(Result.Ok()).Verifiable(Times.Once());
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<SetupRadarrIntegrationCommand>(), CancellationToken.None))
            .ReturnsAsync(Result.Fail<RadarrIntegration>("Radarr setup failed"))
            .Verifiable(Times.Once());
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<SetupSonarrIntegrationCommand>(), CancellationToken.None))
            .ReturnsAsync(
                (SetupSonarrIntegrationCommand command, CancellationToken _) =>
                    Result.Ok(CreateSonarr(command.IntegrationId))
            )
            .Verifiable(Times.Once());

        // Act
        var firstResult = await TestHandlerExecuteAsync(new MigrateLegacyArrSettingsCommand());
        var secondResult = await TestHandlerExecuteAsync(new MigrateLegacyArrSettingsCommand());

        // Assert
        firstResult.IsSuccess.ShouldBeTrue();
        secondResult.IsSuccess.ShouldBeTrue();
        (await IDbContext.RadarrIntegrations.CountAsync(CancellationToken)).ShouldBe(1);
        (await IDbContext.SonarrIntegrations.CountAsync(CancellationToken)).ShouldBe(1);
        (await IDbContext.RadarrIntegrations.SingleAsync(CancellationToken)).ProvisioningState.ShouldBe(
            IntegrationProvisioningState.Unconfigured
        );
        Mock.Mock<IConfigManager>().Verify();
        Mock.Mock<ICommandExecutor>().Verify();
    }

    private static RadarrIntegration CreateRadarr(Guid id) =>
        new()
        {
            Id = id,
            DisplayName = "Radarr",
            BaseUrl = "https://radarr.example.com",
            RadarrApiKey = "radarr-key",
            QBittorrentApiKey = "qbt_23456789ABCDEFGHIJKLMNPQ",
            TorznabApiKey = "0123456789abcdef0123456789abcdef",
            Category = "reaparr-radarr",
            DownloadFolderId = FolderTypeDefaults.DefaultDownloadFolderId,
            ProvisioningState = IntegrationProvisioningState.Unconfigured,
        };

    private static SonarrIntegration CreateSonarr(Guid id) =>
        new()
        {
            Id = id,
            DisplayName = "Sonarr",
            BaseUrl = "https://sonarr.example.com",
            SonarrApiKey = "sonarr-key",
            QBittorrentApiKey = "qbt_23456789ABCDEFGHIJKLMNPQ",
            TorznabApiKey = "0123456789abcdef0123456789abcdef",
            Category = "reaparr-sonarr",
            DownloadFolderId = FolderTypeDefaults.DefaultDownloadFolderId,
            ProvisioningState = IntegrationProvisioningState.Unconfigured,
        };
}
