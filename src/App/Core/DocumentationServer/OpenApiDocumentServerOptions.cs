namespace ITExpert.OpenApi.Server.Core.DocumentationServer
{
    public class OpenApiDocumentServerOptions
    {
        public string MockServerHost { get; set; }
        public bool SkipWrite { get; set; }

        public string SpecsDirectory { get; set; }
    }
}