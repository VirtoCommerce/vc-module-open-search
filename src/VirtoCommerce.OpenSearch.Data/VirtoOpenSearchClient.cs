using OpenSearch.Client;

namespace VirtoCommerce.OpenSearch.Data;
public class VirtoOpenSearchClient : OpenSearchClient
{
    public VirtoOpenSearchClient(IConnectionSettingsValues connectionSettingsValues)
        : base(connectionSettingsValues)
    {

    }
}
