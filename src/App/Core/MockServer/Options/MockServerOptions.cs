using System;
using System.Collections.Generic;

using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.Core.MockServer.Options
{
    public class MockServerOptions
    {
        public string Host { get; set; }

        public Dictionary<string, MockServerOptionsRoute> Routes { get; set; } =
            new Dictionary<string, MockServerOptionsRoute>();

        public Func<OpenApiDocument, string> GetRoutePrefix { get; set; }
    }
}