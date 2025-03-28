using System.Collections.Generic;
using System.Linq;
using OpenSearch.Client;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Model;
using OpenSearchRequest = OpenSearch.Client.SearchRequest;
using SearchRequest = VirtoCommerce.SearchModule.Core.Model.SearchRequest;

namespace VirtoCommerce.OpenSearch.Data;
public class OpenSearchRequestBuilder
{
    protected const string Score = "score";

    public virtual ISearchRequest BuildRequest(SearchRequest request, string indexName, IProperties availableFields)
    {
        var result = new OpenSearchRequest(indexName)
        {
            Query = GetQuery(request),
            PostFilter = GetFilters(request, availableFields),
            Aggregations = GetAggregations(request, availableFields),
        };

        if (request != null)
        {
            result.Sort = GetSorting(request.Sorting);
            result.From = request.Skip;
            result.Size = request.Take;
            result.TrackScores = request.Sorting?.Any(x => x.FieldName.EqualsInvariant(Score)) ?? false;

            if (request.IncludeFields?.Any() == true)
            {
                result.Source = GetSourceFilters(request);
            }

            if (request.Take == 1)
            {
                result.TrackTotalHits = true;
            }
        }

        return result;
    }

    protected virtual SourceFilter GetSourceFilters(SearchRequest request)
    {
        return request?.IncludeFields != null
            ? new SourceFilter { Includes = request.IncludeFields.ToArray() }
            : null;
    }

    protected virtual QueryContainer GetQuery(SearchRequest request)
    {
        QueryContainer result = null;

        if (!string.IsNullOrEmpty(request?.SearchKeywords))
        {
            var keywords = request.SearchKeywords;
            var fields = request.SearchFields?.Select(OpenSearchHelper.ToOpenSearchFieldName).ToArray() ?? new[] { "_all" };

            var multiMatch = new MultiMatchQuery
            {
                Fields = fields,
                Query = keywords,
                Analyzer = "standard",
                Operator = Operator.And
            };

            if (request.IsFuzzySearch)
            {
                multiMatch.Fuzziness = request.Fuzziness != null ? Fuzziness.EditDistance(request.Fuzziness.Value) : Fuzziness.Auto;
            }

            result = multiMatch;
        }

        return result;
    }

    protected virtual IList<ISort> GetSorting(IEnumerable<SortingField> fields)
    {
        var result = fields?.Select(GetSortingField).ToArray();
        return result;
    }

    protected virtual ISort GetSortingField(SortingField field)
    {
        ISort result;

        if (field is GeoDistanceSortingField geoSorting)
        {
            result = new GeoDistanceSort
            {
                Field = OpenSearchHelper.ToOpenSearchFieldName(field.FieldName),
                Points = new[] { geoSorting.Location.ToGeoLocation() },
                Order = geoSorting.IsDescending ? SortOrder.Descending : SortOrder.Ascending,
            };
        }
        else if (field.FieldName.EqualsInvariant(Score))
        {
            result = new FieldSort
            {
                Field = new Field("_score"),
                Order = field.IsDescending ? SortOrder.Descending : SortOrder.Ascending
            };
        }
        else
        {
            result = new FieldSort
            {
                Field = OpenSearchHelper.ToOpenSearchFieldName(field.FieldName),
                Order = field.IsDescending ? SortOrder.Descending : SortOrder.Ascending,
                Missing = "_last",
                UnmappedType = FieldType.Long
            };
        }

        return result;
    }

    protected virtual QueryContainer GetFilters(SearchRequest request, IProperties availableFields)
    {
        return GetFilterQueryRecursive(request?.Filter, availableFields);
    }

    protected virtual QueryContainer GetFilterQueryRecursive(IFilter filter, IProperties availableFields)
    {
        QueryContainer result = null;

        switch (filter)
        {
            case IdsFilter idsFilter:
                result = CreateIdsFilter(idsFilter);
                break;

            case TermFilter termFilter:
                result = CreateTermFilter(termFilter, availableFields);
                break;

            case RangeFilter rangeFilter:
                result = CreateRangeFilter(rangeFilter);
                break;

            case GeoDistanceFilter geoDistanceFilter:
                result = CreateGeoDistanceFilter(geoDistanceFilter);
                break;

            case NotFilter notFilter:
                result = CreateNotFilter(notFilter, availableFields);
                break;

            case AndFilter andFilter:
                result = CreateAndFilter(andFilter, availableFields);
                break;

            case OrFilter orFilter:
                result = CreateOrFilter(orFilter, availableFields);
                break;

            case WildCardTermFilter wildcardTermFilter:
                result = CreateWildcardTermFilter(wildcardTermFilter);
                break;
        }

        return result;
    }

    protected virtual QueryContainer CreateIdsFilter(IdsFilter idsFilter)
    {
        QueryContainer result = null;

        if (idsFilter?.Values != null)
        {
            result = new IdsQuery { Values = idsFilter.Values.Select(id => new Id(id)) };
        }

        return result;
    }

    protected virtual QueryContainer CreateWildcardTermFilter(WildCardTermFilter wildcardTermFilter)
    {
        return new WildcardQuery
        {
            Field = OpenSearchHelper.ToOpenSearchFieldName(wildcardTermFilter.FieldName),
            Value = wildcardTermFilter.Value
        };
    }

    protected virtual QueryContainer CreateTermFilter(TermFilter termFilter, IProperties availableFields)
    {
        var termValues = termFilter.Values;

        var field = availableFields.Where(kvp => kvp.Key.Name.EqualsInvariant(termFilter.FieldName)).Select(kvp => kvp.Value).FirstOrDefault();
        if (field?.Type?.EqualsInvariant(FieldType.Boolean.ToString()) == true)
        {
            termValues = termValues.Select(v => v switch
            {
                "1" => "true",
                "0" => "false",
                _ => v.ToLowerInvariant()
            }).ToArray();
        }

        return new TermsQuery
        {
            Field = OpenSearchHelper.ToOpenSearchFieldName(termFilter.FieldName),
            Terms = termValues
        };
    }

    protected virtual QueryContainer CreateRangeFilter(RangeFilter rangeFilter)
    {
        QueryContainer result = null;

        var fieldName = OpenSearchHelper.ToOpenSearchFieldName(rangeFilter.FieldName);
        foreach (var value in rangeFilter.Values)
        {
            result |= CreateTermRangeQuery(fieldName, value);
        }

        return result;
    }

    protected virtual QueryContainer CreateGeoDistanceFilter(GeoDistanceFilter geoDistanceFilter)
    {
        return new GeoDistanceQuery
        {
            Field = OpenSearchHelper.ToOpenSearchFieldName(geoDistanceFilter.FieldName),
            Location = geoDistanceFilter.Location.ToGeoLocation(),
            Distance = new Distance(geoDistanceFilter.Distance, DistanceUnit.Kilometers),
        };
    }

    protected virtual QueryContainer CreateNotFilter(NotFilter notFilter, IProperties availableFields)
    {
        QueryContainer result = null;

        if (notFilter?.ChildFilter != null)
        {
            result = !GetFilterQueryRecursive(notFilter.ChildFilter, availableFields);
        }

        return result;
    }

    protected virtual QueryContainer CreateAndFilter(AndFilter andFilter, IProperties availableFields)
    {
        QueryContainer result = null;

        if (andFilter?.ChildFilters != null)
        {
            foreach (var childQuery in andFilter.ChildFilters)
            {
                result &= GetFilterQueryRecursive(childQuery, availableFields);
            }
        }

        return result;
    }

    protected virtual QueryContainer CreateOrFilter(OrFilter orFilter, IProperties availableFields)
    {
        QueryContainer result = null;

        if (orFilter?.ChildFilters != null)
        {
            foreach (var childQuery in orFilter.ChildFilters)
            {
                result |= GetFilterQueryRecursive(childQuery, availableFields);
            }
        }

        return result;
    }

    protected virtual TermRangeQuery CreateTermRangeQuery(string fieldName, RangeFilterValue value)
    {
        var lower = string.IsNullOrEmpty(value.Lower) ? null : value.Lower;
        var upper = string.IsNullOrEmpty(value.Upper) ? null : value.Upper;
        return CreateTermRangeQuery(fieldName, lower, upper, value.IncludeLower, value.IncludeUpper);
    }

    protected virtual TermRangeQuery CreateTermRangeQuery(string fieldName, string lower, string upper, bool includeLower, bool includeUpper)
    {
        var termRangeQuery = new TermRangeQuery { Field = fieldName };

        if (includeLower)
        {
            termRangeQuery.GreaterThanOrEqualTo = lower;
        }
        else
        {
            termRangeQuery.GreaterThan = lower;
        }

        if (includeUpper)
        {
            termRangeQuery.LessThanOrEqualTo = upper;
        }
        else
        {
            termRangeQuery.LessThan = upper;
        }

        return termRangeQuery;
    }

    protected virtual AggregationDictionary GetAggregations(SearchRequest request, IProperties availableFields)
    {
        var result = new Dictionary<string, AggregationContainer>();

        if (request?.Aggregations != null)
        {
            foreach (var aggregation in request.Aggregations)
            {
                var aggregationId = aggregation.Id ?? aggregation.FieldName;
                var fieldName = OpenSearchHelper.ToOpenSearchFieldName(aggregation.FieldName);

                if (IsRawKeywordField(fieldName, availableFields))
                {
                    fieldName += ".raw";
                }

                var filter = GetFilterQueryRecursive(aggregation.Filter, availableFields);

                if (aggregation is TermAggregationRequest termAggregationRequest)
                {
                    AddTermAggregationRequest(result, aggregationId, fieldName, filter, termAggregationRequest);
                }
                else if (aggregation is RangeAggregationRequest rangeAggregationRequest)
                {
                    AddRangeAggregationRequest(result, aggregationId, fieldName, filter, rangeAggregationRequest.Values);
                }
            }
        }

        return result.Count != 0 ? new AggregationDictionary(result) : null;
    }

    protected static bool IsRawKeywordField(string fieldName, IProperties availableFields)
    {
        return availableFields
            .Any(kvp =>
                kvp.Key.Name.EqualsInvariant(fieldName) &&
                kvp.Value is KeywordProperty keywordProperty &&
                keywordProperty.Fields?.ContainsKey("raw") == true);
    }

    protected virtual void AddTermAggregationRequest(IDictionary<string, AggregationContainer> container, string aggregationId, string field, QueryContainer filter, TermAggregationRequest termAggregationRequest)
    {
        var facetSize = termAggregationRequest.Size;

        TermsAggregation termsAggregation = null;

        if (!string.IsNullOrEmpty(field))
        {
            termsAggregation = new TermsAggregation(aggregationId)
            {
                Field = field,
                Size = facetSize == null
                        ? null
                        : facetSize > 0 ? facetSize : int.MaxValue,
            };

            if (termAggregationRequest.Values?.Any() == true)
            {
                termsAggregation.Include = new TermsInclude(termAggregationRequest.Values);
            }
        }

        if (filter == null)
        {
            if (termsAggregation != null)
            {
                container.Add(aggregationId, termsAggregation);
            }
        }
        else
        {
            var filterAggregation = new FilterAggregation(aggregationId) { Filter = filter };

            if (termsAggregation != null)
            {
                filterAggregation.Aggregations = termsAggregation;
            }

            container.Add(aggregationId, filterAggregation);
        }
    }

    protected virtual void AddRangeAggregationRequest(Dictionary<string, AggregationContainer> container, string aggregationId, string fieldName, QueryContainer filter, IEnumerable<RangeAggregationRequestValue> values)
    {
        if (values == null)
        {
            return;
        }

        foreach (var value in values)
        {
            var aggregationValueId = $"{aggregationId}-{value.Id}";
            var query = CreateTermRangeQuery(fieldName, value.Lower, value.Upper, value.IncludeLower, value.IncludeUpper);

            var filterAggregation = new FilterAggregation(aggregationValueId)
            {
                Filter = new BoolQuery
                {
                    Must = new List<QueryContainer> { query, filter }
                }
            };

            container.Add(aggregationValueId, filterAggregation);
        }
    }
}
