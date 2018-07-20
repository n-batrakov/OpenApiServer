using System;
using System.Collections.Generic;
using System.Linq;

namespace ITExpert.OpenApi.Server.Core.MockServer.Validation
{
    public class RequestValidationStatus : IEquatable<RequestValidationStatus>
    {
        public bool IsSuccess { get; }

        public bool IsFaulty => !IsSuccess;

        public IEnumerable<RequestValidationError> Errors { get; }

        public RequestValidationStatus(IReadOnlyCollection<RequestValidationError> errors)
        {
            IsSuccess = errors == null || errors.Count == 0;
            Errors = errors;
        }

        public static RequestValidationStatus Success() =>
                new RequestValidationStatus(new RequestValidationError[0]);

        public static RequestValidationStatus Error(params RequestValidationError[] errors) =>
                new RequestValidationStatus(errors);


        public bool Equals(RequestValidationStatus other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var isErrorsEqual = Errors == null && other.Errors == null ||
                                Errors != null && other.Errors != null &&
                                (
                                    Errors.Equals(other.Errors) ||
                                    Errors.SequenceEqual(other.Errors)
                                );

            return IsSuccess == other.IsSuccess && isErrorsEqual;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((RequestValidationStatus)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (IsSuccess.GetHashCode() * 397) ^ (Errors != null ? Errors.GetHashCode() : 0);
            }
        }
    }
}