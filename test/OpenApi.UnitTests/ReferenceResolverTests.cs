using ITExpert.OpenApiServer.Exceptions;
using ITExpert.OpenApiServer.Util;

using Microsoft.OpenApi.Models;

using Xunit;

namespace OpenApiServer.UnitTests
{
    public class ReferenceResolverTests
    {
        private ReferenceResolver Sut { get; }

        public ReferenceResolverTests()
        {
            Sut = new ReferenceResolver(TestData.Petstore);
        }

        [Fact]
        public void CanResolveRealReference()
        {
            var path = TestData.Petstore.Paths["/pets"];
            var operation = path.Operations[OperationType.Get];
            var response = operation.Responses["default"];
            var media = response.Content["application/json"];
            var reference = media.Schema.Reference;

            var actual = Sut.Resolve<OpenApiSchema>(reference);
            var expected = TestData.Petstore.Components.Schemas["Error"];

            Assert.Same(expected, actual);
        }

        [Fact]
        public void ThrowOnMissingReference()
        {
            var reference = new OpenApiReference {Id = "_", Type = ReferenceType.Schema};

            Assert.Throws<OpenApiFormatException>(() => Sut.Resolve<OpenApiSchema>(reference));
        }
    }
}