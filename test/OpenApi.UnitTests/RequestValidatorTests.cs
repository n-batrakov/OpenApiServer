using ITExpert.OpenApi.Server.Internals;
using ITExpert.OpenApi.Server.MockServer.Types;

using OpenApiServer.UnitTests.Utils;

using Xunit;

using ValidationError = ITExpert.OpenApi.Server.Internals.ValidationError;

namespace OpenApiServer.UnitTests
{
    public class RequestValidatorTests
    {
        private RequestValidator Sut { get; }

        public RequestValidatorTests()
        {
            Sut = new RequestValidator();
        }

        [Fact]
        public void CanValidateRequest()
        {
            var ctx = RequestBuilder.FromUrl("/pets").Build();
            var spec = TestData.Petstore.Get("/pets");

            var actual = Sut.Validate(ctx, spec);
            var expected = RequestValidationStatus.Success();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanInvalidateRequestWithoutRequiredQueryParameter()
        {
            var ctx = RequestBuilder.FromUrl("/pets").Build();
            var spec = TestData.Petstore.Get("/pets").ConfiugureParameter("tags", x => x.Required = true);

            var actual = Sut.Validate(ctx, spec);
            var expected = ValidationError.ParameterRequired("tags").AsStatus();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanValidateRequestWithRequiredQueryParameter()
        {
            var ctx = RequestBuilder.FromUrl("/pets?tags=test").Build();
            var spec = TestData.Petstore.Get("/pets").ConfiugureParameter("tags", x => x.Required = true);

            var actual = Sut.Validate(ctx, spec);
            var expected = RequestValidationStatus.Success();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanInvalidateRequestWithMistypedParameter()
        {
            var ctx = RequestBuilder.FromUrl("/pets?limit=false").Build();
            var spec = TestData.Petstore.Get("/pets");

            var actual = Sut.Validate(ctx, spec);
            var expected = TestData
                           .GetInvalidParameterTypeSchemaError("Integer", "String")
                           .Wrap(x => ValidationError.InvalidParameter("limit", x))
                           .AsStatus();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanValidateRequestWithBody()
        {
            var body = "{\"name\": \"name\", \"tag\": \"tag\"}";
            var ctx = RequestBuilder.FromUrl("/pets").WithBody(body).Build();
            var spec = TestData.Petstore.Post("/pets");

            var actual = Sut.Validate(ctx, spec);
            var expected = RequestValidationStatus.Success();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanInvalidateRequestWithInvalidBody()
        {
            var body = "{\"tag\": \"tag\"}";
            var ctx = RequestBuilder.FromUrl("/pets").WithBody(body).Build();
            var spec = TestData.Petstore.Post("/pets");

            var actual = Sut.Validate(ctx, spec);
            var expected = TestData.GetMissingParameterSchemaError("name")
                                   .Wrap(x => ValidationError.InvalidBody(x))
                                   .AsStatus();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanInvalidateRequestWithoutRequiredBody()
        {
            var ctx = RequestBuilder.FromUrl("/pets").Build();
            var spec = TestData.Petstore.Post("/pets");

            var actual = Sut.Validate(ctx, spec);
            var expected = ValidationError.BodyRequired().AsStatus();

            Assert.Equal(expected, actual);
        }

        //TODO: Test Path, Form, Headers
    }
}