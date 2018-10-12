using System;

using OpenApiServer.Core.MockServer.Validation.Types;

namespace UnitTests.Utils
{
    public static class RequestStatusExtensions
    {
        public static HttpValidationError Wrap(this HttpValidationError wrapped,
                                                  Func<HttpValidationError, HttpValidationError> callback) =>
                callback(wrapped);

        public static HttpValidationStatus AsStatus(this HttpValidationError error) =>
                HttpValidationStatus.Error(error);
    }
}