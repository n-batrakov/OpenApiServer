using System;
using System.Collections.Generic;
using System.Linq;

using ITExpert.OpenApi.Server.Core.MockServer.Internals.ResponseGeneration;

using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.Core.MockServer.Internals
{
    internal static class ParametersExtensions
    {
        private const string ObjectNotSupported =
                "Object parameters are not supported just yet for query, header, cookie and path parameters.";

        public static object GetValue(this OpenApiParameter parameter, StringValues values)
        {
            if (parameter.In == null)
            {
                throw new FormatException("Unable to find required 'In' property for parameter.");
            }

            if (parameter.In == ParameterLocation.Path)
            {
                throw new NotImplementedException("Path parameters validation are not implemented just yet.");
            }

            
            var style = parameter.Style ?? GetDefaultStyle(parameter.In.Value);
            var delimeter = GetDelimeter(style);
            var explode = values.Count > 1;
            var type = parameter.Schema.ConvertTypeToEnum();

            switch (type)
            {
                case OpenApiSchemaType.Any:
                case OpenApiSchemaType.String:
                case OpenApiSchemaType.Boolean:
                case OpenApiSchemaType.Integer:
                case OpenApiSchemaType.Number:
                    return ParseScalarValue(values, parameter.Schema);

                case OpenApiSchemaType.Array:
                    return explode
                                   ? ParseArrayValue(values, parameter.Schema)
                                   : ParseArrayValue(values.Single().Split(delimeter), parameter.Schema);

                case OpenApiSchemaType.Object:
                    throw new NotImplementedException(ObjectNotSupported);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static object ParseScalarValue(string value, OpenApiSchema schema)
        {
            var hasResult = TryParseToRequiredType(value, schema, out var result);
            return hasResult ? result : value;
        }

        private static IEnumerable<object> ParseArrayValue(IEnumerable<string> values, OpenApiSchema schema)
        {
            return values.Select(x => ParseScalarValue(x, schema.Items)).ToArray();
        }

        private static ParameterStyle GetDefaultStyle(ParameterLocation location)
        {
            switch(location)
            {
                case ParameterLocation.Query:
                    return ParameterStyle.Form;
                case ParameterLocation.Header:
                    return ParameterStyle.Simple;
                case ParameterLocation.Path:
                    return ParameterStyle.Simple;
                case ParameterLocation.Cookie:
                    return ParameterStyle.Form;
                default:
                    throw new ArgumentOutOfRangeException(nameof(location), location, null);
            }
        }

        private static char GetDelimeter(ParameterStyle style)
        {
            switch (style)
            {
                case ParameterStyle.Form:
                case ParameterStyle.Simple:
                    return ',';
                case ParameterStyle.SpaceDelimited:
                    return ' ';
                case ParameterStyle.PipeDelimited:
                    return '|';

                case ParameterStyle.Matrix:
                case ParameterStyle.Label:
                case ParameterStyle.DeepObject:
                    throw new NotSupportedException();
                default:
                    throw new ArgumentOutOfRangeException(nameof(style), style, null);
            }
        }

        private static bool TryParseToRequiredType(string value, OpenApiSchema schema, out object result)
        {
            var type = schema.ConvertTypeToEnum();
            switch (type)
            {
                case OpenApiSchemaType.Boolean:
                    var hasResult = bool.TryParse(value, out var boolean);
                    result = boolean;
                    return hasResult;
                case OpenApiSchemaType.Integer:
                    hasResult = int.TryParse(value, out var integer);
                    result = integer;
                    return hasResult;
                case OpenApiSchemaType.Number:
                    hasResult = double.TryParse(value, out var number);
                    result = number;
                    return hasResult;

                case OpenApiSchemaType.String:
                    result = value;
                    return true;
                case OpenApiSchemaType.Any:
                    result = value;
                    return false;
                case OpenApiSchemaType.Null:
                    result = null;
                    return value == null;

                case OpenApiSchemaType.Combined:
                    object combined = null;
                    var schemas = schema.AnyOf.Concat(schema.OneOf).Concat(schema.OneOf);
                    hasResult = schemas.Any(x => TryParseToRequiredType(value, x, out combined));
                    result = combined;
                    return hasResult;

                case OpenApiSchemaType.Object:
                case OpenApiSchemaType.Array:
                    result = null;
                    return false;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}