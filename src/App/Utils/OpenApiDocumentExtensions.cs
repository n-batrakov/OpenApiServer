using System;
using System.Collections.Generic;
using System.Linq;

using ITExpert.OpenApiServer.Exceptions;

using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApiServer.Utils
{
    public static class OpenApiDocumentExtensions
    {
        public static T ResolveReference<T>(this OpenApiDocument doc, OpenApiReference reference)
        {
            if (reference.IsLocal && reference.Type != null)
            {
                return (T)ResolveLocalByType(doc, reference.Id, reference.Type.Value);
            }

            if (reference.IsExternal)
            {
                throw new NotSupportedException("External references are not supported just yet.");
            }

            throw new FormatException($"Invalid reference: '{reference.Id}'");
        }

        private static object ResolveLocalByType(OpenApiDocument doc, string id, ReferenceType type)
        {
            var lookup = doc.Components;
            switch (type)
            {
                case ReferenceType.Schema:
                    return GetValue(lookup.Schemas);
                case ReferenceType.Response:
                    return GetValue(lookup.Responses);
                case ReferenceType.Parameter:
                    return GetValue(lookup.Parameters);
                case ReferenceType.Example:
                    return GetValue(lookup.Examples);
                case ReferenceType.RequestBody:
                    return GetValue(lookup.RequestBodies);
                case ReferenceType.Header:
                    return GetValue(lookup.Headers);
                case ReferenceType.SecurityScheme:
                    return GetValue(lookup.SecuritySchemes);
                case ReferenceType.Link:
                    return GetValue(lookup.Links);
                case ReferenceType.Callback:
                    return GetValue(lookup.Callbacks);
                case ReferenceType.Tag:
                    return doc.Tags.FirstOrDefault(x => x.Name.Equals(id, StringComparison.OrdinalIgnoreCase));
                default:
                    throw new ArgumentOutOfRangeException();
            }

            object GetValue<T>(IDictionary<string, T> source)
            {
                var hasKey = source.TryGetValue(id, out var result);
                if (hasKey)
                {
                    return result;
                }

                throw new OpenApiFormatException($"Unable to resolve '#/components/{type}s/{id}'");
            }
        }
    }
}