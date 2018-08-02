using System;

using ITExpert.OpenApi.Server.Core.MockServer.Validation.Types;

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