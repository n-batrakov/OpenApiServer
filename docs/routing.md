# Routing

## Path Components

* Original Host
    - required: Proxy, SwaggerUI
    - in: Global Servers, Operation Servers
* Prefix
    - required: Proxy, Mock, SwaggerUI
    - in: Global Servers, Operation Servers
* Path
    - required: Proxy, Mock
    - in: Operation Path


## Path Variances

1. Absolute path in Servers
    - Take host
    - Take prefix
2. Relative path in Servers
    - Take host from route config or header
    - Take path as prefix
3. Servers is empty
    - Take host from route config or header
    - Use default prefix
4. No host defined anywhere
    - If proxying return [mock | error]
5. No prefix defined in Servers
    - Take host from Servers
    - Use empty prefix

The host is resolved in following order:
1. Header
2. Config
3. Operation Servers
4. Global Servers

*When host cannot be found anywhere, proxing is disabled and default prefix is used.*


## Hosting Variances

1. Server with proxy:
    - Original Host may be passed in headers
2. Server without proxy or Localhost
    - Original host must be specified in spec or config

## Context

- PathTemplate (Operation path as is)
- PathAndQuery (as in request)
- PathPrefix
- OriginalHost


Usage:
1. Router: `             PathPrefix + PathTemplate`
2. Prox:   `OriginalHost + PathPrefix + PathAndQuery`