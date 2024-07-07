namespace Tool.GenerateAst
{
    public class TreeConfiguration
    {
        public string Namespace { get; set; } = string.Empty;
        public Dictionary<string, string> Placeholders { get; set; } = [];
        public Dictionary<string, TreeConfigurationDomain> Domains { get; set; } = [];
    }

    public class TreeConfigurationDomain
    {
        public string Visitor { get; set; } = string.Empty;
        public string Interface { get; set; } = string.Empty;
        public string ParameterIdentifier { get; set; } = string.Empty;

        public List<string> Definitions { get; set; } = [];
    }
}
