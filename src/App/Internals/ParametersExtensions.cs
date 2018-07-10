using System;
using System.Linq;

using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.Internals
{
    public static class ParametersExtensions
    {
        public static object GetValue(this OpenApiParameter parameter, StringValues values)
        {
            if (parameter.Explode)
            {
                return values.ToArray();
            }

            if (parameter.Style.HasValue)
            {
                return GetArrayValue(parameter.Style.Value, values.SingleOrDefault());
            }

            return values.SingleOrDefault();
        }

        private static object GetArrayValue(ParameterStyle style, string arrayString)
        {
            if (string.IsNullOrEmpty(arrayString))
            {
                return null;
            }

            switch (style)
            {
                case ParameterStyle.Label:
                    return arrayString.Substring(1).Split(',').ToArray();
                case ParameterStyle.Simple:
                case ParameterStyle.Form:
                    return arrayString.Split(',').ToArray();
                case ParameterStyle.SpaceDelimited:
                    return arrayString.Split(' ').ToArray();
                case ParameterStyle.PipeDelimited:
                    return arrayString.Split('|').ToArray();

                case ParameterStyle.Matrix:
                case ParameterStyle.DeepObject:
                    throw new NotSupportedException();

                default:
                    throw new ArgumentOutOfRangeException(nameof(style), style, null);
            }
        }
    }
}