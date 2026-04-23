using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Extensions.DependencyInjection;

namespace Reaparr.Build;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var services = CreateServiceCollection();
        using var registrar = new DependencyInjectionRegistrar(services);
        using var bootstrapProvider = services.BuildServiceProvider();
        var logger = bootstrapProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Reaparr.Build.Program");

        try
        {
            var app = new CommandApp(registrar);

            app.Configure(config =>
            {
                config.SetApplicationName("reaparr-build");
                config.UseStrictParsing();
                config.PropagateExceptions();
                config.CaseSensitivity(CaseSensitivity.None);
                config.SetApplicationVersion(typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0");

                config.AddExample(
                    "desktop",
                    "publish",
                    "--rid",
                    "linux-x64",
                    "--version",
                    "0.0.1",
                    "--informational-version",
                    "0.0.1-local"
                );
                config.AddExample(
                    "desktop",
                    "package",
                    "--rid",
                    "osx-arm64",
                    "--version",
                    "0.0.1",
                    "--informational-version",
                    "0.0.1-local",
                    "--dry-run"
                );
                config.AddExample(
                    "desktop",
                    "run",
                    "--rid",
                    "win-x64",
                    "--version",
                    "0.0.1",
                    "--informational-version",
                    "0.0.1-local"
                );
                config.AddExample(
                    "desktop",
                    "ci-package",
                    "--rid",
                    "linux-x64",
                    "--version",
                    "0.0.1",
                    "--informational-version",
                    "0.0.1-dev.1",
                    "--frontend-public-dir",
                    ".tmp/frontend-public",
                    "--artifact-dir",
                    "Releases"
                );

                config.AddBranch(
                    "desktop",
                    desktop =>
                    {
                        desktop
                            .AddCommand<DesktopPublishCommand>("publish")
                            .WithDescription("Generate frontend assets and publish AppHost for a desktop RID.")
                            .WithExample(
                                "desktop",
                                "publish",
                                "--rid",
                                "linux-x64",
                                "--version",
                                "0.0.1",
                                "--informational-version",
                                "0.0.1-local"
                            );

                        desktop
                            .AddCommand<DesktopPackageCommand>("package")
                            .WithDescription("Publish AppHost and create Velopack artifacts for a desktop RID.")
                            .WithAlias("pack")
                            .WithExample(
                                "desktop",
                                "package",
                                "--rid",
                                "osx-arm64",
                                "--version",
                                "0.0.1",
                                "--informational-version",
                                "0.0.1-local",
                                "--dry-run"
                            );

                        desktop
                            .AddCommand<DesktopCiPackageCommand>("ci-package")
                            .WithDescription(
                                "Use pre-generated frontend assets and perform the RID-specific restore and packaging steps for a desktop build in CI."
                            )
                            .WithExample(
                                "desktop",
                                "ci-package",
                                "--rid",
                                "linux-x64",
                                "--version",
                                "0.0.1",
                                "--informational-version",
                                "0.0.1-dev.1",
                                "--frontend-public-dir",
                                ".tmp/frontend-public",
                                "--artifact-dir",
                                "Releases"
                            );

                        desktop
                            .AddCommand<DesktopRunCommand>("run")
                            .WithDescription(
                                "Publish or package as needed, then launch the desktop app when supported."
                            )
                            .WithExample(
                                "desktop",
                                "run",
                                "--rid",
                                "win-x64",
                                "--version",
                                "0.0.1",
                                "--informational-version",
                                "0.0.1-local"
                            );
                    }
                );

                config.SetExceptionHandler(
                    (ex, _) =>
                    {
                        logger.LogError(ex, "{Message}", ex.Message);
                        return -1;
                    }
                );
            });

            return await app.RunAsync(args);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{Message}", ex.Message);
            return 1;
        }
    }

    private static IServiceCollection CreateServiceCollection()
    {
        var services = new ServiceCollection();
        return services
            .AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddSimpleConsole(options =>
                {
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                });
            })
            .AddSingleton(BuildPaths.FromCurrentDirectory());
    }
}
