using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenSearch.Client;
using VirtoCommerce.OpenSearch.Data;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;
using Xunit;

namespace VirtoCommerce.OpenSearch.Tests
{
    [Trait("Category", "Unit")]
    public class PropertyConverterTests : SearchProviderTestsBase
    {
        public static IEnumerable<object[]> TestData
        {
            get
            {
                var entity = new TestEntity();
                var entities = new[] { entity };

                yield return new object[] { "entity", entity };
                yield return new object[] { "array", entities };
                yield return new object[] { "list", entities.ToList() };
                yield return new object[] { "enumerable", entities.Select(x => x) };
            }
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void CanConvertEntityToNestedProperty(string name, object value)
        {
            var provider = GetTestOpenSearchProvider();
            var result = provider.CreateProviderFieldByType(new IndexDocumentField(name, value, IndexDocumentFieldValueType.Complex));
            Assert.IsType<NestedProperty>(result);
        }

        protected override ISearchProvider GetSearchProvider()
        {
            return GetTestOpenSearchProvider();
        }

        protected TestOpenSearchProvider GetTestOpenSearchProvider()
        {
            var searchOptions = Options.Create(new SearchOptions());
            var openSearchOptions = Options.Create(new OpenSearchOptions { Server = "localhost:9200" });
            var connectionSettings = new OpenSearchConnectionSettings(openSearchOptions);
            var client = new OpenSearchClient(connectionSettings);
            var loggerFactory = LoggerFactory.Create(builder => { builder.ClearProviders(); });
            var logger = loggerFactory.CreateLogger<TestOpenSearchProvider>();

            var provider = new TestOpenSearchProvider(searchOptions, GetSettingsManager(), client, new OpenSearchRequestBuilder(), logger);

            return provider;
        }

        public class TestEntity : Entity
        {
        }

        public class TestOpenSearchProvider : OpenSearchProvider
        {
            public TestOpenSearchProvider(
                IOptions<SearchOptions> searchOptions,
                ISettingsManager settingsManager,
                IOpenSearchClient client,
                OpenSearchRequestBuilder requestBuilder,
                ILogger<TestOpenSearchProvider> logger)
                : base(searchOptions, settingsManager, client, requestBuilder, logger)
            {
            }

            public new IProperty CreateProviderFieldByType(IndexDocumentField field)
            {
                return base.CreateProviderFieldByType(field);
            }
        }
    }
}
