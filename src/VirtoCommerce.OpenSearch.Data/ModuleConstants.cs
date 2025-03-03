using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.OpenSearch.Data;
public static class ModuleConstants
{
    public const string ProviderName = "OpenSearch";

    public static class Settings
    {
        public static class Indexing
        {
#pragma warning disable S109
            public static SettingDescriptor IndexTotalFieldsLimit { get; } = new()
            {
                Name = "VirtoCommerce.Search.OpenSearch.IndexTotalFieldsLimit",
                GroupName = "Search|OpenSearch",
                ValueType = SettingValueType.Integer,
                DefaultValue = 1000,
            };

            public static SettingDescriptor TokenFilter { get; } = new()
            {
                Name = "VirtoCommerce.Search.OpenSearch.TokenFilter",
                GroupName = "Search|OpenSearch",
                ValueType = SettingValueType.ShortText,
                DefaultValue = "custom_edge_ngram",
            };

            public static SettingDescriptor MinGram { get; } = new()
            {
                Name = "VirtoCommerce.Search.OpenSearch.NGramTokenFilter.MinGram",
                GroupName = "Search|OpenSearch",
                ValueType = SettingValueType.Integer,
                DefaultValue = 1,
            };

            public static SettingDescriptor MaxGram { get; } = new()
            {
                Name = "VirtoCommerce.Search.OpenSearch.NGramTokenFilter.MaxGram",
                GroupName = "Search|OpenSearch",
                ValueType = SettingValueType.Integer,
                DefaultValue = 20,
            };

            public static SettingDescriptor DeleteDuplicateIndexes { get; } = new()
            {
                Name = "VirtoCommerce.Search.OpenSearch.DeleteDuplicateIndexes",
                GroupName = "Search|OpenSearch",
                ValueType = SettingValueType.Boolean,
                DefaultValue = true,
            };
#pragma warning restore S109

            public static IEnumerable<SettingDescriptor> AllIndexingSettings
            {
                get
                {
                    yield return IndexTotalFieldsLimit;
                    yield return TokenFilter;
                    yield return MinGram;
                    yield return MaxGram;
                    yield return DeleteDuplicateIndexes;
                }
            }
        }

        public static IEnumerable<SettingDescriptor> AllSettings => Indexing.AllIndexingSettings;
    }
}
