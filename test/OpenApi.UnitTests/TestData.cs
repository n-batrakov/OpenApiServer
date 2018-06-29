using System.IO;

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace OpenApiServer.UnitTests
{
    public static class TestData
    {
        public static readonly OpenApiDocument Petstore = GetDocument("data/petstore.yml");

        private static OpenApiDocument GetDocument(string path)
        {
            var reader = new OpenApiStreamReader();
            using (var streamReader = new StreamReader(path))
            {
                return reader.Read(streamReader.BaseStream, out _);
            }
        }
    }
}