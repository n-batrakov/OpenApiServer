using System;

namespace ITExpert.OpenApi.Core.MockServer.Generation
{
    public class ValueGeneratorNotFoundException : Exception
    {
        public ValueGeneratorNotFoundException() : base("Unable to find suitable generator.")
        {
            
        }
    }
}