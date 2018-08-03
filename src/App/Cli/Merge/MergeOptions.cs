namespace ITExpert.OpenApi.Cli.Merge
{
    public class MergeOptions
    {
        public string Output { get; set; }
        public string Root { get; set; }
        public string Format { get; set; }
        public string Version { get; set; }
        public bool Recursive { get; set; }
        public bool Flat { get; set; }
    }
}