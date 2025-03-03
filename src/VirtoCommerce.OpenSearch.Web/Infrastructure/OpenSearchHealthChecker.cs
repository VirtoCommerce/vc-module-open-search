using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using OpenSearch.Client;
using OpenSearch.Net;
using VirtoCommerce.OpenSearch.Data;

namespace VirtoCommerce.OpenSearch.Web.Infrastructure;

public class OpenSearchHealthChecker : IHealthCheck
{
    private readonly IOpenSearchClient _openSearchClient;
    private readonly OpenSearchOptions _openSearchOptions;

    public OpenSearchHealthChecker(IOpenSearchClient openSearchClient, IOptions<OpenSearchOptions> openSearchOptions)
    {
        _openSearchClient = openSearchClient;
        _openSearchOptions = openSearchOptions.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        // Use different timeout for the healthchecking pings. Otherwise ping will hangs on default timeout so much longer (default is minute and more).
        var pingResult = await _openSearchClient.PingAsync(
            new PingRequest() { RequestConfiguration = new RequestConfiguration() { RequestTimeout = TimeSpan.FromSeconds(_openSearchOptions.HealthCheckTimeout) } },
            cancellationToken);

        if (pingResult.IsValid)
        {
            return HealthCheckResult.Healthy("OpenSearch server is reachable");
        }
        else
        {
            return HealthCheckResult.Unhealthy(@$"No connection to OpenSearch-server.{Environment.NewLine}{pingResult.ApiCall.DebugInformation}");
        }
    }
}
