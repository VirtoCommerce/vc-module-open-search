using System.Globalization;
using OpenSearch.Client;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.OpenSearch.Data;
public static class OpenSearchHelper
{
    /// <summary>
    /// Name of the sub-aggregation holding the min/max statistics of a range aggregation.
    /// </summary>
    public const string StatsAggregationName = "stats";

    public static string ToOpenSearchFieldName(string originalName)
    {
        return originalName?.ToLowerInvariant();
    }

    /// <summary>
    /// Key of the aggregation wrapping <see cref="StatsAggregationName"/>. The request and response builders must agree on it.
    /// </summary>
    public static string ToStatsAggregationId(string aggregationId)
    {
        return $"{aggregationId}-{StatsAggregationName}";
    }

    public static string ToStringInvariant(this object value)
    {
        return string.Format(CultureInfo.InvariantCulture, "{0}", value);
    }

    public static object ToOpenSearchValue(this GeoPoint point)
    {
        return point == null ? null : new { lat = point.Latitude, lon = point.Longitude };
    }

    public static GeoLocation ToGeoLocation(this GeoPoint point)
    {
        return point == null ? null : new GeoLocation(point.Latitude, point.Longitude);
    }
}
