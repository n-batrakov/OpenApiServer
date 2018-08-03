using System.Collections.Generic;
using System.Linq;

using ITExpert.OpenApi.Core.MockServer.Context.Types;
using ITExpert.OpenApi.Utils;

using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

using Newtonsoft.Json.Schema;

namespace ITExpert.OpenApi.Core.MockServer.Context.Mapping
{
    public static class RequestContextSpecConverter
    {
        public static RequestContextSpec ConvertSpec(OpenApiOperation operation,
                                                     IEnumerable<OpenApiServer> globalServers)
        {
            var schemaConverter = new OpenApiSchemaConverter();

            var parameters = MapParameters(operation, schemaConverter).ToArray();
            var requestBody = MapBody(operation, schemaConverter).ToArray();
            var responses = MapResponses(operation, schemaConverter).ToArray();
            var servers = operation.Servers.Concat(globalServers).Select(x => x.FormatUrl()).ToArray();
            return new RequestContextSpec(parameters, requestBody, responses, servers);
        }

        private static IEnumerable<RequestContextParameter> MapParameters(
                OpenApiOperation operation,
                OpenApiSchemaConverter schemaConverter)
        {
            foreach (var parameter in operation.Parameters)
            {
                yield return new RequestContextParameter
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

        private static IEnumerable<RequestContextBody> MapBody(OpenApiOperation operation,
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
                yield return new RequestContextBody(contentType, required, schema, examples);
            }
        }

        private static IEnumerable<RequestContextResponse> MapResponses(OpenApiOperation operation,
                                                                        OpenApiSchemaConverter schemaConverter)
        {
            foreach (var (statusCode, responseSpec) in operation.Responses)
            {
                if (responseSpec.Content.Count == 0)
                {
                    yield return new RequestContextResponse("*/*", statusCode, new JSchema(), new string[0]);
                    yield break;
                }

                foreach (var (contentType, mediaTypeSpec) in responseSpec.Content)
                {
                    var schema = schemaConverter.Convert(mediaTypeSpec.Schema);
                    var examples = GetExamples(mediaTypeSpec.Examples, mediaTypeSpec.Example);
                    yield return new RequestContextResponse(contentType, statusCode, schema, examples);
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