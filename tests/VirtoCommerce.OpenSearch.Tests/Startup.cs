using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace VirtoCommerce.OpenSearch.Tests;

public class Startup
{
    public static void ConfigureHost(IHostBuilder hostBuilder)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<OpenSearchTests>()
            .AddEnvironmentVariables()
            .Build();

        hostBuilder.ConfigureHostConfiguration(builder => builder.AddConfiguration(configuration));
    }
}
