using System;

using ITExpert.OpenApiServer.Exceptions;
using ITExpert.OpenApiServer.Utils;

using Microsoft.OpenApi.Models;

using OpenApiServer.UnitTests.Utils;

using Xunit;

namespace OpenApiServer.UnitTests
{
    public class ReferenceResolverTests
    {
        [Fact]
        public void CanResolveRealReference()
        {
            var path = TestData.Petstore.Paths["/pets"];
            var operation = path.Operations[OperationType.Get];
            var response = operation.Responses["default"];
            var media = response.Content["application/json"];
            var reference = media.Schema.Reference;

            var actual = TestData.Petstore.ResolveReference<OpenApiSchema>(reference);
            var expected = TestData.Petstore.Components.Schemas["Error"];

            Assert.Same(expected, actual);
        }

        [Fact]
        public void ThrowOnMissingReference()
        {
            var reference = new OpenApiReference {Id = "_", Type = ReferenceType.Schema};

            Assert.Throws<OpenApiFormatException>((Action)Callback);

            void Callback() =>
                    TestData.Petstore.ResolveReference<OpenApiSchema>(reference);
        }
    }
}