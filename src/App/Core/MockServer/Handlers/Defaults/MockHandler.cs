using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.OpenApi.Writers;

using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Core.MockServer.Context.Types.Spec;
using OpenApiServer.Core.MockServer.MockDataProviders;
using OpenApiServer.Utils;

namespace OpenApiServer.Core.MockServer.Handlers.Defaults
{
    [RequestHandler("mock")]
    public class MockHandler : IRequestHandler
    {
        private IMockDataProvider DataProvider { get; }

        public MockHandler(IMockDataProvider dataProvider)
        {
            DataProvider = dataProvider;
        }

        public Task<ResponseContext> HandleAsync(RouteContext request)
        {
            var responseSpec = ChooseResponse(request.Spec.Responses);
            return Task.FromResult(responseSpec == null
                                           ? RespondWithNothing()
                                           : RespondWithMock(responseSpec));
        }

        private static RouteSpecResponse ChooseResponse(IEnumerable<RouteSpecResponse> responseSpec)
        {
            var filterMediaType =
                    responseSpec.Where(x => x.ContentType == "*/*" || x.ContentType == "application/json").ToArray();
            if (filterMediaType.Length == 0)
            {
                throw new NotSupportedException("Unable to find suitable response spec. MockServer only supports 'application/json' or '*/*' for now.");
            }

            var comparison = StringComparison.OrdinalIgnoreCase;
            var successResponse = filterMediaType.FirstOrDefault(x => x.StatusCode.StartsWith("2", comparison) ||
                                                                      x.StatusCode.Equals("default", comparison));

            return successResponse ?? filterMediaType.FirstOrDefault();
        }

        private ResponseContext RespondWithMock(RouteSpecResponse mediaType)
        {
            var body = OpenApiSerializer.Serialize(WriteBody);

            return new ResponseContext
            {

                StatusCode = mediaType.StatusCodeParsed,
                ContentType = mediaType.ContentType,
                Body = body,
            };

            void WriteBody(IOpenApiWriter writer)
            {
                var _ = TryWriteExample(writer) || DataProvider.TryWriteValue(writer, mediaType.Schema);
            }

            bool TryWriteExample(IOpenApiWriter writer)
            {
                if (mediaType.Examples?.Count > 0)
                {
                    var example = mediaType.Examples.First();
                    writer.WriteRaw(example);
                    return true;
                }

                return false;
            }
        }

        private static ResponseContext RespondWithNothing() =>
                new ResponseContext { StatusCode = HttpStatusCode.NoContent };
    }
}