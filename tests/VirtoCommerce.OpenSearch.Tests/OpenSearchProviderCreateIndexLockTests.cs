using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using OpenSearch.Client;
using VirtoCommerce.OpenSearch.Data;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.Model;
using Xunit;

namespace VirtoCommerce.OpenSearch.Tests
{
    [Trait("Category", "Unit")]
    public class OpenSearchProviderCreateIndexLockTests
    {
        [Fact]
        public async Task InternalCreateIndexWithLockAsync_PreventsDuplicateIndexCreation()
        {
            // Arrange
            var indexStoreWithoutLock = new IndexStore(new Barrier(2));
            var providerWithoutLock = new TestOpenSearchProvider(indexStoreWithoutLock);

            // Act
            await Task.WhenAll(
                providerWithoutLock.CallInternalCreateIndexAsync(),
                providerWithoutLock.CallInternalCreateIndexAsync());

            // Assert
            indexStoreWithoutLock.CreatedIndexCount.Should().Be(2, "without a lock both calls pass the index existence check before the index is created");

            var indexStoreWithLock = new IndexStore();
            var providerWithLock = new TestOpenSearchProvider(indexStoreWithLock);

            await Task.WhenAll(
                providerWithLock.CallInternalCreateIndexWithLockAsync(),
                providerWithLock.CallInternalCreateIndexWithLockAsync());

            indexStoreWithLock.CreatedIndexCount.Should().Be(1, "the lock should serialize index creation and prevent duplicate indexes");
        }

        [Fact]
        public async Task SwapIndexAsync_SharesLockWithIndexCreation()
        {
            // Arrange
            var indexStore = new IndexStore();
            var provider = new TestOpenSearchProvider(indexStore);

            // Act
            await Task.WhenAll(
                provider.SwapIndexAsync(TestOpenSearchProvider.DocumentType),
                provider.CallInternalCreateIndexWithLockAsync(),
                provider.SwapIndexAsync(TestOpenSearchProvider.DocumentType),
                provider.CallInternalCreateIndexWithLockAsync());

            // Assert
            indexStore.MaxConcurrency.Should().Be(1, "swap and index creation must not run concurrently for the same document type");
            indexStore.CreatedIndexCount.Should().Be(1);
        }

        private sealed class TestOpenSearchProvider : OpenSearchProvider
        {
            public const string DocumentType = "Product";
            private readonly IndexStore _indexStore;

            public TestOpenSearchProvider(IndexStore indexStore)
                : base(
                    Options.Create(new SearchOptions { Scope = "test-core", Provider = "OpenSearch" }),
                    Mock.Of<ISettingsManager>(),
                    new OpenSearchClient(new Uri("http://localhost:9200")),
                    new OpenSearchRequestBuilder(),
                    NullLogger<OpenSearchProvider>.Instance,
                    new PassThroughDistributedLockService())
            {
                _indexStore = indexStore;
            }

            public Task CallInternalCreateIndexAsync()
            {
                return InternalCreateIndexAsync(DocumentType, [], new IndexingParameters());
            }

            public Task CallInternalCreateIndexWithLockAsync()
            {
                return InternalCreateIndexWithLockAsync(DocumentType, [], new IndexingParameters());
            }

            protected override Task InternalSwapIndexAsync(string documentType)
            {
                return _indexStore.TrackConcurrencyAsync(() => Task.CompletedTask);
            }

            protected override Task DeleteDuplicateIndexes(string documentType)
            {
                return Task.CompletedTask;
            }

            protected override Task<IProperties> GetMappingAsync(string indexName)
            {
                return Task.FromResult<IProperties>(new Properties<IProperties>());
            }

            protected override Task<bool> IndexExistsAsync(string indexName)
            {
                return _indexStore.IndexExistsAsync();
            }

            protected override Task CreateIndexAsync(string indexName, string alias)
            {
                _indexStore.CreateIndex();

                return Task.CompletedTask;
            }

            protected override Task UpdateMappingAsync(string indexName, IProperties properties)
            {
                return Task.CompletedTask;
            }
        }

        private sealed class IndexStore
        {
            private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(5);
            private readonly Barrier _indexExistsBarrier;
            private int _createdIndexCount;
            private int _concurrency;
            private int _maxConcurrency;

            public IndexStore(Barrier indexExistsBarrier = null)
            {
                _indexExistsBarrier = indexExistsBarrier;
            }

            public int CreatedIndexCount => Volatile.Read(ref _createdIndexCount);

            public int MaxConcurrency => Volatile.Read(ref _maxConcurrency);

            public Task<bool> IndexExistsAsync()
            {
                return TrackConcurrencyAsync(() =>
                {
                    var indexExists = CreatedIndexCount > 0;
                    _indexExistsBarrier?.SignalAndWait(_timeout);

                    return Task.FromResult(indexExists);
                });
            }

            public void CreateIndex()
            {
                Interlocked.Increment(ref _createdIndexCount);
            }

            public async Task TrackConcurrencyAsync(Func<Task> action)
            {
                await TrackConcurrencyAsync(async () =>
                {
                    await action();
                    return true;
                });
            }

            /// <summary>
            /// Runs the action and records how many actions overlap
            /// </summary>
            public async Task<T> TrackConcurrencyAsync<T>(Func<Task<T>> action)
            {
                var concurrency = Interlocked.Increment(ref _concurrency);
                UpdateMaxConcurrency(concurrency);

                try
                {
                    await Task.Yield();
                    await Task.Delay(20);

                    return await action();
                }
                finally
                {
                    Interlocked.Decrement(ref _concurrency);
                }
            }

            private void UpdateMaxConcurrency(int concurrency)
            {
                int currentMax;
                do
                {
                    currentMax = Volatile.Read(ref _maxConcurrency);
                    if (concurrency <= currentMax)
                    {
                        return;
                    }
                }
                while (Interlocked.CompareExchange(ref _maxConcurrency, concurrency, currentMax) != currentMax);
            }
        }
    }
}
