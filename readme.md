# OpenApiServer

CLI Tools to help you work with OpenAPI Specifications (aka "Swagger").

Features:
* Embedded SwaggerUI - allows viewing the documentation
* MockServer - allows calling REST API described by the specification and to receive generated data in response.
* Proxy - allows mixing response mocking with actual API call, including partial mocking (actual and mock API responses are merged together).
* Request \ Response validation - incoming requests and proxy responses may be tested for specification compliance.
* Allows writing specs for each route in a separate file. All spec files are combined together at startup and grouped by `info.title` and `info.version`.
* Supports OAS3 in JSON or YAML (OAS2 may work, but it's not tested.).

## Installation

### NPM

```shell
npm install --global @lasmera/oas
```

### dotnet tool

> Requires .net core sdk 2.1 and above.

```shell
dotnet tool install -g lasmera.oas
```

### docker

Run OpenApiServer on port `5000` with configs located at `./.oas`.

```shell
docker run -p 5000:80 -v .oas:/app/.oas lasmera/oas run
```

The following file structure is assumed:

```
oas/
├── spec/
|   ├── example_spec_1.yml
|   └── example_spec_2.yaml
|   └── example_spec_3.json
└── oas.config.json
```

### binaries

1. [Download](https://github.com/Lasmera/OpenApiServer/releases) binaries for your platform.
2. Unpack.
3. Run `./oas --version` in directory with unpacked binaries.

## Getting started

OpenApiServer works with OpenAPI Specification version 3 in JSON and YAML formats.

Before getting started you should have spec files ready.
See [Specification](https://github.com/OAI/OpenAPI-Specification)
or [samples](https://github.com/OAI/OpenAPI-Specification/tree/master/examples/v3.0)
to get acquainted with the concept.

### CLI

To start OpenApiServer from your terminal simply run

```
oas run
```

This command starts the server listening on port `5000`,
with the config file located at `./.oas/oas.config.json`.
Spec files are recursively searched in `./.oas/specs` directory.
If config file is not found it is created with the default parameters.

You may specify port, config location and other parameters via command line arguments.
See `oas run --help` for details.

When server is launched, you may see available API via SwaggerUI at `http://localhost:5000` (by default).

### Configuration and specs

OpenApiServer configuration via JSON file located at `./.oas/oas.config.json` by default.

OpenApiServer watches for changes made to this file at runtime. 
So any configuration changes made to this file is instantly available without server restart.
Unlike spec files, which are not watched, so changes to them will not be reflected until the server is restarted.

Router building:
* Search - 
    Specified spec directory is recursively scanned for all `*.json`, `*.yaml` and `*.yml` files. 

    The ones that recognized as OpenAPI specification files are selected.
* Conversion -
    Selected spec files converted to OAS3 JSON files.
* Spec merging -
    Selected and converted specs are grouped by `info` section (`title` and `version`).

    Each group then merged into one spec file.
    
    If different files have overlapping elements, they overwrite each other.
* Configuration -
    Each config rule is matched against each spec.
    Every matching configs then are merged together (see config merging below).
* Add route - 
    Each spec with config becomes OpenApiServer route.

Config merging:
* Each spec with its path and method, is matched against config's `path` and `method`.
* Matched routes are then combined together in **descending** order (lower the index - higher the precedence).
* *At the moment* config "above" (higher precedence) totally overrides the one bellow.

```typescript
type config = {
    // Use for remote server deployment.
    // Should point to public OpenApiServer address.
    // Used by swagger UI to load specs.
    mockServerHost: string;

    // Routing settings
    routes: Array<{

        // Path glob pattern.
        path: string;

        // Http method; use "any" or omit the setting to match any method.
        method: string;

        // Handler used to process the request (see the list of available handlers below).
        handler: string;

        // Handler options. See handlers documentation.
        [handlerOptions: string]: any;
    }>;
}
```

#### Sample 

> See `default` handler documentation for details.

```js
{
    "mockServerHost": "http://localhost:35200",
    "routes": [
        {
            "path": "**",
            "method": "any",
            "validate": "all",
            "mock":  "replace",
            "proxy": "http://localhost:5001",
            "delay": 0
        }
    ]   
}
```



## Handlers

Handlers are the way to configure route's behavior.

Each handler accepts HTTP request, does it's processing logic and returns HTTP response.

Handlers are parameterized by options supplied with route configuration.

Each route must have exactly one handler. 
However, there are special 'combination' handlers like `pipeline` and `merge`, which accept other handlers as options to combine their behavior.

Following is the list of available handlers and their description.

### `default`

`default` handler is a 'syntactic sugar' over `pipeline` handler.

It enables all basic handlers with convenient parameters.

Options:
* `validate`: (`none | request | response | all`) - request \ response validation mode
* `mock`: (`none | merge | replace`) - mocking mode
* `proxy`: (`string`) - actual API address. See `proxy` handler for details.
* `delay`: (`number`) - minimal request execution time in milliseconds

Example

```json
{
    "routes": [
        {
            "handler": "default",
            "validate": "all",
            "mock":  "replace",
            "proxy": "http://localhost:5123",
            "delay": 100
        }
    ]   
}
```

Basically the same as:

```json
{
    "handler": "pipeline",
    "handlers": [
        {
            "handler": "validateRequest"
        }, 
        {
            "handler": "merge",
            "handlers": [
                {
                    "handler": "mock"
                }, 
                {
                    "disable": true,
                    "handler": "proxy",
                    "host": "http://localhost:5123"
                }
            ]
        },
        {
            "handler": "delay",
            "value": 100
        },
        {
            "handler": "validateResponse"
        }
    ]
}
```

### `delay`

Waits for specified amount of milliseconds.

Options: 
* `value`: (`number`) - number of milliseconds to wait

Example

```json
{
    "handler": "delay",
    "value": 100
}
```

### `merge`

Merges multiple JSON responses into one.

Conditions required to merge two responses:
* Both responses have bodies
* Both responses have `application/json` Content-Type
* Both responses have the same status code

If any one of these conditions does not apply, the second response is returned.
I.e. greater the index - higher the precedence.
Consider the example below. If `mock` handler returns 200 application/json 
and the `proxy` handler returns 401 without a body, the latter will be returned to the client.

Options:
* `handlers`: (`{ handlers: { handler: string, [option: string]: any } }`)

```json
{
    "handler": "merge",
    "handlers": [{
        "handler": "mock",
        "disable": false
    }, {
        "handler": "proxy",
        "host": "http://localhost:50123"
    }]
}
```

### `mock`

`mock` handler takes the route spec and returns a response based on its description (status code, content type, schema).

`mock` handler prefers success JSON responses. If none found, the first response used.

Examples in specification are always preferred. 
If the schema has an `example` generation is skipped, and this example is returned.

**For now, mock handler only supports JSON.**

> No options yet

### `pipeline`

Pipeline handler combines multiple handlers into one, invoking them sequentially with the same request and response from the previous handler.
The responses from all handlers are then combined into one.

Handlers from an array in config are invoked in index ascending order (from 0 to handlers.length).

Options: 
* `handlers`: (`{ handlers: { handler: string, [option: string]: any } }`)

```json
{
    "handler": "pipeline",
    "handlers": [{
        "handler": "validateRequest",
        "disable": true
    }, {
        "handler": "proxy",
        "host": "http://localhost:50123"
    }]
}
```

### `proxy`

Proxy handler accepts the request, makes the same request to the actual API and returns the response to the client.

To do that the handler requires actual API address, which is searched in various locations (in order of precedence):
* `X-Forwarded-Host` header
* `host` handler option
* `servers` section in the specification (the first server is used)

> Server Templating OAS3 feature is not currently supported.

Options:
* `host`: (`string`) - actual API address.

### `validateRequest`

Validates the request based on route specification.

> No options yet

### `validateResponse`

Validates the response based on route specification.

> No options yet

## Caveats and Limitations

* Currently only JSON is supported for most of the operations.
* Only HTTP(-S) protocol supported.
* Many of the JSON schema features are ignored when mocking (for example string patterns, object's pattern properties, etc).
* Response validation is not implemented.
* Requires validation only validates query, form, and body (no headers and path).
* References to files are not currently supported.