using System.Collections.Generic;
using System.Linq;

using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.Context.Internals;
using OpenApiServer.Core.MockServer.Context.Types.Spec;
using OpenApiServer.Utils;

namespace OpenApiServer.Core.MockServer.Context.Mapping
{
    public static class RequestContextSpecConverter
    {
        public static RouteSpec ConvertSpec(OpenApiOperation operation,
                                                     IEnumerable<Microsoft.OpenApi.Models.OpenApiServer> globalServers)
        {
            var schemaConverter = new OpenApiSchemaConverter();

            var parameters = MapParameters(operation, schemaConverter).ToArray();
            var requestBody = MapBody(operation, schemaConverter).ToArray();
            var responses = MapResponses(operation, schemaConverter).ToArray();
            var servers = operation.Servers.Concat(globalServers).Select(x => x.FormatUrl()).ToArray();
            return new RouteSpec(parameters, requestBody, responses, servers);
        }

        private static IEnumerable<RouteSpecRequestParameter> MapParameters(
                OpenApiOperation operation,
                OpenApiSchemaConverter schemaConverter)
        {
            foreach (var parameter in operation.Parameters)
            {
                yield return new RouteSpecRequestParameter
                             {
                                     Style = parameter.Style,
                                     AllowEmptyValue = parameter.AllowEmptyValue,
                                     In = parameter.In ?? ParameterLocation.Query,
                                     Required = parameter.Required,
                                     Name = parameter.Name,
                                     Explode = parameter.Explode,
                                     Examples = GetExamples(parameter.Examples, parameter.Example),
                                     Schema = schemaConverter.Convert(parameter.Schema)
                             };
            }
        }

        private static IEnumerable<RouteSpecRequestBody> MapBody(OpenApiOperation operation,
                                                               OpenApiSchemaConverter schemaConverter)
        {
            if (operation.RequestBody == null)
            {
                yield break;
            }

            var required = operation.RequestBody.Required;

            foreach (var (contentType, body) in operation.RequestBody.Content)
            {
                var schema = schemaConverter.Convert(body.Schema);
                var examples = GetExamples(body.Examples, body.Example);
                yield return new RouteSpecRequestBody(contentType, required, schema, examples);
            }
        }

        private static IEnumerable<RouteSpecResponse> MapResponses(OpenApiOperation operation,
                                                                        OpenApiSchemaConverter schemaConverter)
        {
            foreach (var (statusCode, responseSpec) in operation.Responses)
            {
                if (responseSpec.Content.Count == 0)
                {
                    yield return new RouteSpecResponse("*/*", statusCode, new JSchema(), new string[0]);
                    yield break;
                }

                foreach (var (contentType, mediaTypeSpec) in responseSpec.Content)
                {
                    var schema = schemaConverter.Convert(mediaTypeSpec.Schema);
                    var examples = GetExamples(mediaTypeSpec.Examples, mediaTypeSpec.Example);
                    yield return new RouteSpecResponse(contentType, statusCode, schema, examples);
                }
            }
        }

        private static IReadOnlyCollection<string> GetExamples(IDictionary<string, OpenApiExample> examples,
                                                               params IOpenApiAny[] additionalExamples) =>
                examples.Select(x => x.Value.Value)
                        .Concat(additionalExamples)
                        .Where(x => x != null)
                        .Select(x => OpenApiSerializer.Serialize(x.Write))
                        .ToArray();
    }
}