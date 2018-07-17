using System;
using System.Collections.Generic;
using System.Linq;

namespace ITExpert.OpenApi.Server.Core.MockServer.Internals.ResponseGeneration
{
    public static class EnumerableExtensions
    {
        private static readonly Random Random = new Random();

        public static IEnumerable<T> TakeRandom<T>(this IEnumerable<T> source, double probability = 0.5) =>
                source.Where(element => Random.NextDouble() <= probability);
    }
}