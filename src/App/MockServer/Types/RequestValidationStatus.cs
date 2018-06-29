using System.Collections.Generic;

namespace ITExpert.OpenApiServer.MockServer.Types
{
    public class RequestValidationStatus
    {
        public bool IsSuccess { get; }

        public bool IsFaulty => !IsSuccess;

        public IEnumerable<RequestValidationError> Errors { get; }

        private RequestValidationStatus(bool isSuccess, IEnumerable<RequestValidationError> errors)
        {
            IsSuccess = isSuccess;
            Errors = errors;
        }

        public static RequestValidationStatus Success() => 
                new RequestValidationStatus(true, null);

        public static RequestValidationStatus Error(params RequestValidationError[] errors) =>
                new RequestValidationStatus(false, errors);
    }
}