# `oas run`

Usage:
```
oas run [OPTIONS] <SOURCES>
```

## Arguments

* `SOURCES` - disk paths or URLs to OpenApi Specification files. 

Disk path may point to 
* file - single spec will be loaded
* directory - specified directory will be scaned for spec files recursively.

Note that many paths may be passed as arguments. E.g.

```
oas run 1.json 2.json 3.json
```

`discover` flag may change arguments behaviour (see options).

## Options

* `-p | --port` - port listened by the server.
* `-D | --discover` - if set, `source` argument treated as discovery file.
* `--discover-key` - ???
* `-v | --verbosity` - minimal log level; one of `trace`, `debug`, `information`, `warning`, `error`, `critical`, `none`.
* `-c | --conifg` - path to config file.


## Discovery

Discovery file is a JSON string containing web URIs to spec files. 

Discovery file is an array of objects where each object has `url` property (string).

However, if `--discover-key` is set, system will search for discovery value in an object with the property name as this parameter value.

### Discovery file format example

```json
[{
    "url": "http://localhost:5234/v1/swagger.json"
},{
    "url": "http://localhost:5234/v2/swagger.json"
}]
```

If `--discover-key` set to `specs`:

```json
{
    "specs": [{
        "url": "http://localhost:5234/v1/swagger.json"
    },{
        "url": "http://localhost:5234/v2/swagger.json"
    }]
}
```

## Examples

1. Run server on port 5000. Search specs in current directory. 

```
oas run .
```

2. Change port to 8080.

```
oas run -p 8080 .
```

3. Change specs directory

```
oas run ./specs
```

4. Use one single spec file

```
oas run ./specs/petstore.json
```

5. Use one single spec file from web

```
oas run http://example.com/petstore.json
```

6. Use spec discovery file

```
oas run --discover ./oas.json
```

7. Use spec discovery file from web

```
oas run --discover http://example.com/specs
```