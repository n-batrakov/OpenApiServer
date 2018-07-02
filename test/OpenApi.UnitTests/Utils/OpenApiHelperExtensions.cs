using System;
using System.Linq;

using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;

using Newtonsoft.Json;

namespace OpenApiServer.UnitTests.Utils
{
    public static class OpenApiHelperExtensions
    {
        public static OpenApiOperation Get(this OpenApiDocument doc, string path) =>
                doc.Paths[path].Operations[OperationType.Get];

        public static OpenApiOperation Post(this OpenApiDocument doc, string path) => 
                doc.Paths[path].Operations[OperationType.Post];

        public static OpenApiOperation ConfiugureParameter(this OpenApiOperation operation,
                                                           string parameter,
                                                           Action<OpenApiParameter> configure)
        {
            operation = Clone(operation);
            var param = operation.Parameters.First(x => x.Name.Equals(parameter, StringComparison.OrdinalIgnoreCase));
            configure(param);
            return operation;
        }

        private static OpenApiOperation Clone(OpenApiOperation operation)
        {
            var json = operation.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
            return JsonConvert.DeserializeObject<OpenApiOperation>(json);
        }
    }
}