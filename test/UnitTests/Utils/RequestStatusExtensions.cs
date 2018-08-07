using System;

using ITExpert.OpenApi.Core.MockServer.Validation;

namespace UnitTests.Utils
{
    public static class RequestStatusExtensions
    {
        public static RequestValidationError Wrap(this RequestValidationError wrapped,
                                                  Func<RequestValidationError, RequestValidationError> callback) =>
                callback(wrapped);

        public static RequestValidationStatus AsStatus(this RequestValidationError error) =>
                RequestValidationStatus.Error(error);
    }
}