namespace VirtoCommerce.OpenSearch.Data;
public class OpenSearchOptions
{
    public string Server { get; set; }
    public string User { get; set; }
    /// <summary>
    /// Configures password.
    /// </summary>
    public string Password { get; set; }
    /// <summary>
    /// Sets **true** value to enables gzip compressed requests and responses or **false** (default).
    /// </summary>
    public bool? EnableHttpCompression { get; set; } = false;

    /// <summary>
    /// Sets the default timeout in seconds for each request to OpenSearch. Defaults to 60 seconds.
    /// </summary>
    public int RequestTimeout { get; set; } = 60;

    /// <summary>
    /// Sets the default timeout in seconds for health checking pings to OpenSearch. Defaults to 2 seconds
    /// </summary>
    public int HealthCheckTimeout { get; set; } = 2;
}
