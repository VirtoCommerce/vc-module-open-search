using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenSearch.Client;
using VirtoCommerce.OpenSearch.Data;
using VirtoCommerce.OpenSearch.Data.Extensions;
using VirtoCommerce.OpenSearch.Web.Infrastructure;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.Extensions;

namespace VirtoCommerce.OpenSearch.Web;

public class Module : IModule, IHasConfiguration
{
    public ManifestModuleInfo ModuleInfo { get; set; }
    public IConfiguration Configuration { get; set; }

    public void Initialize(IServiceCollection serviceCollection)
    {
        if (Configuration.SearchProviderActive(ModuleConstants.ProviderName))
        {
            serviceCollection.Configure<OpenSearchOptions>(Configuration.GetSection($"Search:{ModuleConstants.ProviderName}"));
            serviceCollection.AddTransient<OpenSearchRequestBuilder>();
            serviceCollection.AddSingleton<IConnectionSettingsValues, OpenSearchConnectionSettings>();
            serviceCollection.AddSingleton<IOpenSearchClient, VirtoOpenSearchClient>();
            serviceCollection.AddSingleton<OpenSearchProvider>();
            serviceCollection.AddHealthChecks().AddCheck<OpenSearchHealthChecker>("OpenSearch server connection", tags: ["Modules", "OpenSearch"]);
        }
    }

    public void PostInitialize(IApplicationBuilder appBuilder)
    {
        var settingsRegistrar = appBuilder.ApplicationServices.GetRequiredService<ISettingsRegistrar>();
        settingsRegistrar.RegisterSettings(ModuleConstants.Settings.AllSettings, ModuleInfo.Id);

        if (Configuration.SearchProviderActive(ModuleConstants.ProviderName))
        {
            appBuilder.UseSearchProvider<OpenSearchProvider>(ModuleConstants.ProviderName, (provider, documentTypes) =>
            {
                provider.AddActiveAlias(documentTypes);
            });
        }
    }

    public void Uninstall()
    {
        // Nothing to do here
    }
}
