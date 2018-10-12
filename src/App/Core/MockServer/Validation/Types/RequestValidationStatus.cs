using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenApiServer.Core.MockServer.Validation.Types
{
    public class HttpValidationStatus : IEquatable<HttpValidationStatus>
    {
        public bool IsSuccess { get; }

        public IEnumerable<HttpValidationError> Errors { get; }

        public HttpValidationStatus(IReadOnlyCollection<HttpValidationError> errors)
        {
            IsSuccess = errors == null || errors.Count == 0;
            Errors = errors;
        }

        public static HttpValidationStatus Success() =>
                new HttpValidationStatus(new HttpValidationError[0]);

        public static HttpValidationStatus Error(params HttpValidationError[] errors) =>
                new HttpValidationStatus(errors);


        public bool Equals(HttpValidationStatus other)
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

            return Equals((HttpValidationStatus)obj);
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