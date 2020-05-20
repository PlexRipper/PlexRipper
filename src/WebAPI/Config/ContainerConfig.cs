using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace PlexRipper.WebAPI.Config
{
    public static class ContainerConfig
    {
        public static IConfigurationRoot Setup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            return builder.Build();
        }

    }
}
