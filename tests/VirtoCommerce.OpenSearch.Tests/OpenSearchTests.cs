using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenSearch.Client;
using VirtoCommerce.OpenSearch.Data;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;
using Xunit;

namespace VirtoCommerce.OpenSearch.Tests
{
    [Trait("Category", "CI")]
    [Trait("Category", "IntegrationTest")]
    public class OpenSearchTests : SearchProviderTests
    {
        private readonly IConfiguration _configuration;

        public OpenSearchTests(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override ISearchProvider GetSearchProvider()
        {
            var searchOptions = Options.Create(new SearchOptions { Scope = "test-core", Provider = "OpenSearch" });
            var openSearchOptions = Options.Create(_configuration.GetSection("OpenSearch").Get<OpenSearchOptions>());
            openSearchOptions.Value.Server ??= Environment.GetEnvironmentVariable("TestOpenSearchHost") ?? "localhost:9200";
            var connectionSettings = new OpenSearchConnectionSettings(openSearchOptions);
            var client = new OpenSearchClient(connectionSettings);
            var loggerFactory = LoggerFactory.Create(builder => { builder.ClearProviders(); });
            var logger = loggerFactory.CreateLogger<OpenSearchProvider>();

            var provider = new OpenSearchProvider(searchOptions, GetSettingsManager(), client, new OpenSearchRequestBuilder(), logger);

            return provider;
        }
    }
}
