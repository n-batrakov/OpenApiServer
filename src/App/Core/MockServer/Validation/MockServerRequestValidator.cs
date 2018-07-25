using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.OpenApi.Models;

using Newtonsoft.Json.Linq;

namespace ITExpert.OpenApi.Server.Core.MockServer.Validation
{
    public class MockServerRequestValidator : IMockServerRequestValidator
    {
        public RequestValidationStatus Validate(IMockServerRequestContext context)
        {
            var operation = context.OperationSpec;
            var paramtersErrors = operation.Parameters?.SelectMany(ValidateParameter) ??
                                  Enumerable.Empty<RequestValidationError>();

            var bodyErrors = operation.RequestBody != null
                                     ? ValidateBody(operation.RequestBody, context.Body, context.ContentType)
                                     : Enumerable.Empty<RequestValidationError>();

            var errors = paramtersErrors.Concat(bodyErrors).ToArray();
            return new RequestValidationStatus(errors);

            IEnumerable<RequestValidationError> ValidateParameter(OpenApiParameter x)
            {
                switch (x.In)
                {
                    case ParameterLocation.Query:
                        return ValidateQuery(x, context.Query);
                    case ParameterLocation.Header:
                        return ValidateHeaders(x, context.Headers);
                    case ParameterLocation.Path:
                        return ValidatePath(x, context.Route);
                    case ParameterLocation.Cookie:
                        return ValidateCookie(x);
                    case null:
                        return Enumerable.Empty<RequestValidationError>();
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        

        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private static IEnumerable<RequestValidationError> ValidateHeaders(
                OpenApiParameter parameter,
                IHeaderDictionary headers)
        {
            yield break;
        }

        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private static IEnumerable<RequestValidationError> ValidateCookie(OpenApiParameter parameter)
        {
            yield break;
        }

        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private static IEnumerable<RequestValidationError> ValidatePath(
                OpenApiParameter parameter,
                RouteData routeData)
        {
            yield break;
        }

        private static IEnumerable<RequestValidationError> ValidateQuery(
                OpenApiParameter parameter,
                IQueryCollection query)
        {
            var hasParameter = query.TryGetValue(parameter.Name, out var parameterValues);
            if (!hasParameter)
            {
                if (parameter.Required)
                {
                    yield return ValidationError.ParameterRequired(parameter.Name);
                }

                yield break;
            }

            if (parameterValues.Count == 0 && !parameter.AllowEmptyValue)
            {
                yield return ValidationError.ParameterMustHaveValue(parameter.Name);

                yield break;
            }

            var value = parameter.GetValue(parameterValues);
            var schemaErrors = parameter.Schema.ValidateValue(value).ToArray();
            if (schemaErrors.Any())
            {
                yield return ValidationError.InvalidParameter(parameter.Name, schemaErrors);
            }
        }

        private static IEnumerable<RequestValidationError> ValidateBody(
                OpenApiRequestBody body,
                string bodyString,
                string contentType)
        {
            if (string.IsNullOrEmpty(bodyString) && body.Required)
            {
                yield return ValidationError.BodyRequired();

                yield break;
            }

            var hasContent = TryGetContent(contentType, body, out var content);
            if (!hasContent)
            {
                yield return ValidationError.UnexpectedContentType(contentType);

                yield break;
            }

            var jsonBody = JObject.Parse(bodyString);
            var schemaErrors = content.Schema.ValidateValue(jsonBody).ToArray();
            if (schemaErrors.Any())
            {
                yield return ValidationError.InvalidBody(schemaErrors);
            }
        }

        private static bool TryGetContent(string contentType,
                                          OpenApiRequestBody body,
                                          out OpenApiMediaType result) =>
                body.Content.TryGetValue(contentType, out result) ||
                body.Content.TryGetValue("*/*", out result);

    }
}