using System;
using System.Globalization;
using System.IO;
using System.Linq;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

using Newtonsoft.Json.Linq;

using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Core.MockServer.ExampleProviders.Internals;

namespace OpenApiServer.Core.MockServer.Context.Internals
{
    internal static class RequestContextBuilder
    {
        public static RequestContext Build(HttpContext ctx)
        {
            return new RequestContext
            {
                PathAndQuery = ctx.Request.GetEncodedPathAndQuery(),
                Method = Enum.Parse<HttpMethod>(ctx.Request.Method, ignoreCase: true),

                ContentType = GetContentType(ctx.Request),
                Query = ctx.Request.Query,
                Headers = ctx.Request.Headers,
                Route = ctx.GetRouteData(),
                Body = GetBody(ctx.Request),
            };
        }

        private static JToken GetBody(HttpRequest request)
        {
            return request.HasFormContentType ? ReadForm() : ReadBody();

            JToken ReadForm()
            {
                var obj = new JObject();
                foreach (var (key, value) in request.Form)
                {
                    var propertyValue = ParseRawValue(value);
                    obj.Add(key, propertyValue);
                }

                var fileProperties = request.Form.Files.ToDictionary(x => x.Name, GetFilePropertyValue);
                obj.AddRange(fileProperties, overwriteDuplicateKeys: false);

                return obj;
            }

            JToken ReadBody()
            {
                using (var reader = new StreamReader(request.Body))
                {
                    var body = reader.ReadToEnd();
                    return ParseRawValue(body);
                }
            }
        }

        private static JToken GetFilePropertyValue(IFormFile file)
        {
            using (var memorySteam = new MemoryStream())
            {
                file.CopyTo(memorySteam);
                var bytes = memorySteam.ToArray();
                var base64 = Convert.ToBase64String(bytes);
                return new JValue(base64);
            }
        }

        private static string GetContentType(HttpRequest request)
        {
            var hasContentType = request.Headers.TryGetValue("Content-Type", out var values);
            if (!hasContentType || values.Count == 0)
            {
                return null;
            }

            var contentType = values.First();
            return contentType.Split(';').First();
        }

        private static JToken ParseRawValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            if (value.StartsWith('{') || value.StartsWith('['))
            {
                return JToken.Parse(value);
            }

            if (bool.TryParse(value, out var boolean))
            {
                return new JValue(boolean);
            }

            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var num))
            {
                return new JValue(num);
            }

            return new JValue(value);
        }
    }
}