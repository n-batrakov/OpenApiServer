using System;

using OpenApiServer.Core.MockServer.Exceptions;

namespace OpenApiServer.Core.MockServer.Generation.Types
{
    public class ValueGeneratorNotFoundException : MockServerException
    {
        public ValueGeneratorNotFoundException() : base("Unable to find suitable generator.")
        {
            
        }
    }
}