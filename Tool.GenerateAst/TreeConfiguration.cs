namespace Tool.GenerateAst
{
    public class TreeConfiguration
    {
        public string Namespace { get; set; } = string.Empty;
        public string Expression { get; set; } = string.Empty;
        public string Statement { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Literal { get; set; } = string.Empty;
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
