using System;

using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.Core.MockServer
{
    public class MockServerOptions
    {
        public Func<OpenApiDocument, string> GetRoutePrefix { get; set; }
    }
}