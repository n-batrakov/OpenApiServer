using System;
using System.Collections.Generic;
using System.Linq;

using ITExpert.OpenApi.Server.Core.MockServer.Internals;
using ITExpert.OpenApi.Server.Core.MockServer.Types;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Models;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ITExpert.OpenApi.Server.Core.MockServer
{
    public class RequestValidator
    {
        public RequestValidationStatus Validate(HttpRequestValidationContext context,
                                                OpenApiOperation operation)
        {
            var errors = operation.Parameters.SelectMany(x => ValidateParameter(context, x));
            if (operation.RequestBody != null)
            {
                if (string.IsNullOrEmpty(context.ContentType))
                {
                    context.ContentType = "application/json";
                }
                var bodyErrors = ValidateBody(operation.RequestBody, context.ContentType, context.Body);
                var formErrors = ValidateForm(operation.RequestBody, context.ContentType, context.Form);
                errors = errors.Concat(bodyErrors).Concat(formErrors);
            }

            var errorsArray = errors.ToArray();
            return errorsArray.Any()
                           ? RequestValidationStatus.Error(errorsArray)
                           : RequestValidationStatus.Success();
        }

        private IEnumerable<RequestValidationError> ValidateParameter(HttpRequestValidationContext context,
                                                                      OpenApiParameter parameter)
        {
            if (parameter.In == null)
            {
                throw new FormatException("OpenAPI operation parameter must have 'In' property.");
            }

            switch (parameter.In.Value)
            {
                case ParameterLocation.Query:
                    return ValidateQuery(parameter, context.Query);
                case ParameterLocation.Header:
                    return ValidateHeaders(parameter, context.Headers);
                case ParameterLocation.Path:
                case ParameterLocation.Cookie:
                    //TODO
                    return Enumerable.Empty<RequestValidationError>();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static IEnumerable<RequestValidationError> ValidateQuery(
                OpenApiParameter parameter,
                IQueryCollection parametersValues)
        {
            var dict = new Dictionary<string, StringValues>(parametersValues);
            return ValidateParametersCollection(parameter, dict);
        }

        private static IEnumerable<RequestValidationError> ValidateHeaders(
                OpenApiParameter parameter,
                IHeaderDictionary parametersValues)
        {
            return ValidateParametersCollection(parameter, parametersValues);
        }

        private static IEnumerable<RequestValidationError> ValidateParametersCollection(
                OpenApiParameter parameter,
                IDictionary<string, StringValues> parametersValues)
        {
            if (!parametersValues.ContainsKey(parameter.Name))
            {
                if (parameter.Required)
                {
                    yield return ValidationError.ParameterRequired(parameter.Name);
                }
                
                yield break;
            }

            var requestParameter = parameter.GetValue(parametersValues[parameter.Name]);
            if (requestParameter == null && !parameter.AllowEmptyValue)
            {
                yield return ValidationError.ParameterMustHaveValue(parameter.Name);
                yield break;
            }

            var schemaValidationErrors = parameter.Schema.ValidateValue(requestParameter).ToArray();
            if (schemaValidationErrors.Any())
            {
                yield return ValidationError.InvalidParameter(parameter.Name, schemaValidationErrors);
            }
        }


        private static IEnumerable<RequestValidationError> ValidateBody(
                OpenApiRequestBody bodySpec,
                string contentType,
                string body)
        {
            if (bodySpec.Required && string.IsNullOrEmpty(body))
            {
                yield return ValidationError.BodyRequired();

                yield break;
            }

            var isSpecHasContent = bodySpec.Content.ContainsKey(contentType);
            if (!isSpecHasContent)
            {
                yield return ValidationError.UnexpectedContentType(contentType);

                yield break;
            }


            var content = bodySpec.Content[contentType];
            var jbody = JObject.Parse(body);
            var errors = content.Schema.ValidateValue(jbody).ToArray();
            if (errors.Any())
            {
                yield return ValidationError.InvalidBody(errors);
            }
        }


        private static IEnumerable<RequestValidationError> ValidateForm(
                OpenApiRequestBody bodySpec,
                string contentType,
                IFormCollection form)
        {
            const string formUrlEncoded = "application/x-www-form-urlencoded";
            const string multipartFormData = "multipart/form-data";

            if (bodySpec.Content.ContainsKey(formUrlEncoded) || bodySpec.Content.ContainsKey(multipartFormData))
            {
                var dict = form.ToDictionary(x => x.Key, x => x.Value.ToString());
                var body = JsonConvert.SerializeObject(dict);
                return ValidateBody(bodySpec, contentType, body);
            }

            return Enumerable.Empty<RequestValidationError>();
        }
    }
}