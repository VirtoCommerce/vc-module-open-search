using System.Globalization;
using OpenSearch.Client;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.OpenSearch.Data;
public static class OpenSearchHelper
{
    public static string ToOpenSearchFieldName(string originalName)
    {
        return originalName?.ToLowerInvariant();
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
