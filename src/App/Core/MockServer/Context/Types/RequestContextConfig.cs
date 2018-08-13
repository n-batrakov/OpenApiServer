namespace OpenApiServer.Core.MockServer.Context.Types
{
    public class RequestContextConfig
    {
        public string Handler { get; set; }

        public int Delay { get; set; }

        //public bool Mock { get; set; }

        public string Host { get; set; }

        public bool ValidateRequest { get; set; }
        public bool ValidateResponse { get; set; }
    }
}