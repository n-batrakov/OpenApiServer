using System;

using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace OpenApiServer.Core.MockServer.Context.Types
{
    public readonly struct RouteId : IEquatable<RouteId>
    {
        public string Path { get; }
        public HttpMethod Verb { get; }

        public RouteId(string path, HttpMethod verb)
        {
            Path = path;
            Verb = verb;
        }

        public RouteId(string path, string verb)
        {
            Path = path;
            var hasParsed = Enum.TryParse<HttpMethod>(verb, ignoreCase: true, out var methodEnum);
            if (!hasParsed)
            {
                throw new ArgumentException($"Http method '{verb}' is not recognized.");
            }

            Verb = methodEnum;
        }

        public override string ToString()
        {
            return $"{Path} ({Verb})";
        }

        public bool Equals(RouteId other)
        {
            return string.Equals(Path, other.Path) && Verb == other.Verb;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is RouteId && Equals((RouteId)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Path != null ? Path.GetHashCode() : 0) * 397) ^ (int)Verb;
            }
        }
    }
}