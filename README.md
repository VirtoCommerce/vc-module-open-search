# Virto Commerce OpenSearch Module

[![CI status](https://github.com/VirtoCommerce/vc-module-open-search/workflows/Module%20CI/badge.svg?branch=dev)](https://github.com/VirtoCommerce/vc-module-open-search/actions?query=workflow%3A"Module+CI") [![Quality gate](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-open-search&metric=alert_status&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-open-search) [![Reliability rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-open-search&metric=reliability_rating&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-open-search) [![Security rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-open-search&metric=security_rating&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-open-search) [![Sqale rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-open-search&metric=sqale_rating&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-open-search)

The Virto Commerce OpenSearch module implements the ISearchProvider defined in the VirtoCommerce Search module. It leverages the OpenSearch engines to store indexed documents.

The module supports the following OpenSearch deployment options:

* [OpenSearch](https://opensearch.org/)
* [Amazon OpenSearch Service](https://aws.amazon.com/opensearch-service/)

## Configuration
The OpenSearch Search provider can be configured using the following keys:

* **Search.Provider**: Specifies the search provider name, which must be set to "OpenSearch".
* **Search.Scope**: Specifies the common name (prefix) for all indexes. Each document type is stored in a separate index, and the full index name is scope-{documenttype}. This allows one search service to serve multiple indexes. (Optional: Default value is "default".)
* **Search.OpenSearch.Server**: Specifies the network address and port of the OpenSearch server.
* **Search.OpenSearch.User**: Specifies the username for private OpenSearch server. (Optional: Default value is "openSearch".)
* **Search.OpenSearch.Password**: Specifies the password for either the Elastic Cloud cluster or private OpenSearch server. (Optional)
* **Search.OpenSearch.EnableHttpCompression**: Set this to "true" to enable gzip compressed requests and responses or "false" (default) to disable compression. (Optional)

For more information about configuration settings, refer to the [Virto Commerce Configuration Settings documentation](https://virtocommerce.com/docs/user-guide/configuration-settings/).

## Samples
Here are some sample configurations:

### OpenSearch v2.x
For OpenSearch v2.x without security features enabled:

```json
"Search": {
    "Provider": "OpenSearch",
    "Scope": "default",
    "OpenSearch": {
        "Server": "https://localhost:9200"
    }
}
```
For OpenSearch v2.x with ApiKey authorization:

```json
"Search": {
    "Provider": "OpenSearch",
    "Scope": "default",
    "OpenSearch": {
        "Server": "https://localhost:9200",
        "User": "{USER_NAME}",
        "Password": "{PASSWORD}"
    }
}
```

## Documentation

* [REST API](https://virtostart-demo-admin.govirto.com/docs/index.html?urls.primaryName=VirtoCommerce.Search)
* [View on GitHub](https://github.com/VirtoCommerce/vc-module-open-search)

## References

* [Deployment](https://docs.virtocommerce.org/platform/developer-guide/Tutorials-and-How-tos/Tutorials/deploy-module-from-source-code/)
* [Installation](https://docs.virtocommerce.org/platform/user-guide/modules-installation/)
* [Home](https://virtocommerce.com)
* [Community](https://www.virtocommerce.org)
* [Download latest release](https://github.com/VirtoCommerce/vc-module-open-search/releases/latest)

## License

Copyright (c) Virto Solutions LTD.  All rights reserved.

Licensed under the Virto Commerce Open Software License (the "License"); you
may not use this file except in compliance with the License. You may
obtain a copy of the License at

http://virtocommerce.com/opensourcelicense

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied.
