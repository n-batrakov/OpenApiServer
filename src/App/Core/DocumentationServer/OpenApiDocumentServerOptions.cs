namespace ITExpert.OpenApi.Server.Core.DocumentationServer
{
    public class OpenApiDocumentServerOptions
    {
        public string MockServerHost { get; set; }
        public bool SkipWrite { get; set; }

        public string SpecsDirectory { get; set; }


        public string SwaggerUrl { get; set; } = "";
        public string SpecsUrl { get; set; } = "specs";
        public string SpecFilename { get; set; } = "openapi.json";
        public string SwaggerUi { get; set; }
    }
}