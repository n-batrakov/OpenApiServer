using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using ITExpert.OpenApi.Server.Core.MockServer.Options;
using ITExpert.OpenApi.Server.Core.MockServer.Validation;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.OpenApi.Models;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ITExpert.OpenApi.Server.Core.MockServer
{
    public class ValidateRequestMiddleware
    {
        private IMockServerRequestValidator Validator { get; }
        private RequestDelegate Next { get; }

        public ValidateRequestMiddleware(RequestDelegate next, IMockServerRequestValidator validator)
        {
            Next = next;
            Validator = validator;
        }

        public Task InvokeAsync(HttpContext ctx)
        {
            var mockContext = ctx.GetMockContext();
            return ShouldValidate(mockContext.RouteOptions.Validate)
                           ? Validate(ctx, mockContext.OperationSpec)
                           : Next(ctx);
        }

        private bool ShouldValidate(MockServerOptionsValidationMode mode)
        {
            switch (mode)
            {
                case MockServerOptionsValidationMode.None:
                    return false;
                case MockServerOptionsValidationMode.Request:
                    return true;
                case MockServerOptionsValidationMode.Response:
                    return false;
                case MockServerOptionsValidationMode.All:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        private Task Validate(HttpContext ctx, OpenApiOperation operation)
        {
            var requestCtx = GetRequestContext(ctx.Request, ctx.GetRouteData());
            var validationStatus = Validator.Validate(requestCtx, operation);
            return validationStatus.IsFaulty
                           ? RespondWithBadRequest(ctx.Response, validationStatus.Errors)
                           : Next(ctx);
        }

        private static Task RespondWithBadRequest(HttpResponse response, IEnumerable<RequestValidationError> errors)
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            response.ContentType = "application/json";
            var json = JsonConvert.SerializeObject(errors);
            return response.WriteAsync(json, Encoding.UTF8);
        }

        private static HttpRequestValidationContext GetRequestContext(HttpRequest request, RouteData routeData)
        {
            return new HttpRequestValidationContext
                   {
                           Route = routeData,
                           Headers = request.Headers,
                           Query = request.Query,
                           Body = request.HasFormContentType ? ReadForm() : ReadBody(),
                           ContentType = request.ContentType
                   };

            string ReadForm()
            {
                var dict = request.Form.ToDictionary(x => x.Key, x => JToken.Parse(x.Value));
                return JsonConvert.SerializeObject(dict);
            }

            string ReadBody()
            {
                using (var reader = new StreamReader(request.Body))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}