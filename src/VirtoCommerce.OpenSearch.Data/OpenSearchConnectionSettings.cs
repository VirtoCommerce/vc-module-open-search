using System;
using Microsoft.Extensions.Options;
using OpenSearch.Client;
using OpenSearch.Net;

namespace VirtoCommerce.OpenSearch.Data;
public class OpenSearchConnectionSettings : ConnectionSettings
{
    public OpenSearchConnectionSettings(IOptions<OpenSearchOptions> openSearchOptions)
        : base(GetConnectionPool(openSearchOptions.Value))
    {
        var options = openSearchOptions.Value;
        var userName = options.User;
        var password = options.Password;

        if (!string.IsNullOrEmpty(password))
        {
            // openSearch is default name for openSearch cloud
            BasicAuthentication(userName ?? "openSearch", password);
        }

        if (options.EnableHttpCompression.HasValue && (bool)options.EnableHttpCompression)
        {
            EnableHttpCompression();
        }

        RequestTimeout(TimeSpan.FromSeconds(options.RequestTimeout));
    }

    protected static IConnectionPool GetConnectionPool(OpenSearchOptions options)
    {
        var serverUrl = GetServerUrl(options);

        return new SingleNodeConnectionPool(serverUrl);
    }

    protected static Uri GetServerUrl(OpenSearchOptions options)
    {
        var server = options.Server;

        if (string.IsNullOrEmpty(server))
        {
            throw new ArgumentException("'Server' parameter must not be empty");
        }

        if (!server.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !server.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            server = "http://" + server;
        }

        server = server.TrimEnd('/');

        return new Uri(server);
    }
}
