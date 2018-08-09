using System;

namespace OpenApiServer.Core.MockServer.Generation.Types
{
    public class ValueGeneratorNotFoundException : Exception
    {
        public ValueGeneratorNotFoundException() : base("Unable to find suitable generator.")
        {
            
        }
    }
}