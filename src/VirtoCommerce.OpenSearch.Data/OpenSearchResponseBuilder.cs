using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using OpenSearch.Client;
using VirtoCommerce.SearchModule.Core.Extensions;
using VirtoCommerce.SearchModule.Core.Model;
using SearchRequest = VirtoCommerce.SearchModule.Core.Model.SearchRequest;

namespace VirtoCommerce.OpenSearch.Data;
public static class OpenSearchResponseBuilder
{
    public static SearchResponse ToSearchResponse(this ISearchResponse<SearchDocument> response, SearchRequest request)
    {
        var result = new SearchResponse();

        if (response.Total > 0)
        {
            result.TotalCount = response.Total;
            result.Documents = response.Hits.Select(ToSearchDocument).ToArray();
            result.Aggregations = GetAggregations(response.Aggregations, request);
        }

        return result;
    }

    public static SearchDocument ToSearchDocument(IHit<SearchDocument> hit)
    {
        var result = new SearchDocument { Id = hit.Id };

        // Copy fields and convert JArray to object[]
        var fields = (IDictionary<string, object>)hit.Source ?? (IDictionary<string, object>)hit.Fields;

        if (fields != null)
        {
            foreach (var kvp in fields)
            {
                var name = kvp.Key;
                var value = kvp.Value;

                if (value is JArray jArray)
                {
                    value = jArray.ToObject<object[]>();
                }

                if (value is List<object> list)
                {
                    var convertedList = new List<object>();

                    foreach (var item in list)
                    {
                        if (item is IDictionary<string, object>)
                        {
                            var jObject = JObject.FromObject(item);
                            convertedList.Add(jObject);
                        }
                        else
                        {
                            convertedList.Add(item);
                        }
                    }

                    value = convertedList.ToArray();
                }

                if (value is Dictionary<string, object> dictionary)
                {
                    value = JObject.FromObject(dictionary);
                }

                result.Add(name, value);
            }
        }

        result.SetRelevanceScore(hit.Score);

        return result;
    }

    private static IList<AggregationResponse> GetAggregations(IReadOnlyDictionary<string, IAggregate> searchResponseAggregations, SearchRequest request)
    {
        var result = new List<AggregationResponse>();

        if (request?.Aggregations != null && searchResponseAggregations != null)
        {
            foreach (var aggregationRequest in request.Aggregations)
            {
                var aggregation = new AggregationResponse
                {
                    Id = aggregationRequest.Id ?? aggregationRequest.FieldName,
                    Values = new List<AggregationResponseValue>()
                };

                var rangeAggregationRequest = aggregationRequest as RangeAggregationRequest;

                if (aggregationRequest is TermAggregationRequest)
                {
                    AddAggregationValues(aggregation, aggregation.Id, aggregation.Id, searchResponseAggregations);
                }
                else if (rangeAggregationRequest?.Values != null)
                {
                    foreach (var value in rangeAggregationRequest.Values)
                    {
                        var queryValueId = value.Id;
                        var responseValueId = $"{aggregation.Id}-{queryValueId}";
                        AddAggregationValues(aggregation, responseValueId, queryValueId, searchResponseAggregations);
                    }
                }

                if (aggregation.Values.Any())
                {
                    result.Add(aggregation);
                }
            }
        }

        return result;
    }

    private static void AddAggregationValues(AggregationResponse aggregation, string responseKey, string valueId, IReadOnlyDictionary<string, IAggregate> searchResponseAggregations)
    {
        if (searchResponseAggregations.ContainsKey(responseKey))
        {
            var aggregate = searchResponseAggregations[responseKey];
            var bucketAggregate = aggregate as BucketAggregate;
            var singleBucketAggregate = aggregate as SingleBucketAggregate;

            if (singleBucketAggregate != null)
            {
                if (singleBucketAggregate.ContainsKey(responseKey))
                {
                    bucketAggregate = singleBucketAggregate[responseKey] as BucketAggregate;
                }
                else if (singleBucketAggregate.DocCount > 0)
                {
                    var aggregationValue = new AggregationResponseValue
                    {
                        Id = valueId,
                        Count = singleBucketAggregate.DocCount
                    };

                    aggregation.Values.Add(aggregationValue);
                }
            }

            if (bucketAggregate != null)
            {
                foreach (var term in bucketAggregate.Items.OfType<KeyedBucket<object>>())
                {
                    if (term.DocCount > 0)
                    {
                        var aggregationValue = new AggregationResponseValue
                        {
                            Id = term.KeyAsString ?? term.Key.ToStringInvariant(),
                            Count = term.DocCount ?? 0
                        };

                        aggregation.Values.Add(aggregationValue);
                    }
                }
            }
        }
    }
}
