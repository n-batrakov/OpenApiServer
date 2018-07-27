namespace ITExpert.OpenApi.Server.Core.MockServer.Types
{
    public class RequestContextConfig
    {
        public int Delay { get; set; }
        public bool Mock { get; set; }
        public bool ValidateRequest { get; set; }
        public bool ValidateResponse { get; set; }

        public string Host { get; set; }
    }
}