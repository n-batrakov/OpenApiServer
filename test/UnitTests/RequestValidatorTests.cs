using OpenApiServer.Core.MockServer.Validation;
using OpenApiServer.Core.MockServer.Validation.Types;

using UnitTests.Utils;

using Xunit;

namespace UnitTests
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
            var ctx = RequestBuilder
                      .FromUrl("/pets")
                      .WithSpec(TestData.Petstore.Get("/pets"))
                      .Build();
            

            var actual = Sut.Validate(ctx);
            var expected = HttpValidationStatus.Success();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanInvalidateRequestWithoutRequiredQueryParameter()
        {
            var spec = TestData.Petstore.Get("/pets").ConfiugureParameter("tags", x => x.Required = true);
            var ctx = RequestBuilder.FromUrl("/pets").WithSpec(spec).Build();
            

            var actual = Sut.Validate(ctx);
            var expected = ValidationError.ParameterRequired("tags").AsStatus();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanValidateRequestWithRequiredQueryParameter()
        {
            var spec = TestData.Petstore.Get("/pets").ConfiugureParameter("tags", x => x.Required = true);
            var ctx = RequestBuilder.FromUrl("/pets?tags=test").WithSpec(spec).Build();

            var actual = Sut.Validate(ctx);
            var expected = HttpValidationStatus.Success();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanInvalidateRequestWithMistypedParameter()
        {
            var spec = TestData.Petstore.Get("/pets");
            var ctx = RequestBuilder.FromUrl("/pets?limit=false").WithSpec(spec).Build();

            var actual = Sut.Validate(ctx);
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
            var spec = TestData.Petstore.Post("/pets");
            var ctx = RequestBuilder.FromUrl("/pets").WithBody(body).WithSpec(spec).Build();

            var actual = Sut.Validate(ctx);
            var expected = HttpValidationStatus.Success();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanInvalidateRequestWithInvalidBody()
        {
            var body = "{\"tag\": \"tag\"}";
            var spec = TestData.Petstore.Post("/pets");
            var ctx = RequestBuilder.FromUrl("/pets").WithBody(body).WithSpec(spec).Build();
            
            var actual = Sut.Validate(ctx);
            var expected = TestData.GetMissingParameterSchemaError("name", "'', line 1, position 1")
                                   .Wrap(x => ValidationError.InvalidBody(x))
                                   .AsStatus();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanInvalidateRequestWithoutRequiredBody()
        {
            var spec = TestData.Petstore.Post("/pets");
            var ctx = RequestBuilder.FromUrl("/pets").WithSpec(spec).Build();

            var actual = Sut.Validate(ctx);
            var expected = ValidationError.BodyRequired().AsStatus();

            Assert.Equal(expected, actual);
        }

        //TODO: Test Path, Form, Headers
    }
}