namespace Tool.GenerateAst
{
    public class TreeConfiguration
    {
        public string Namespace { get; set; } = string.Empty;

        public string Visitor { get; set; } = string.Empty;
        public string Expression { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Literal { get; set; } = string.Empty;
        public List<string> Definitions { get; set; } = [];
    }
}
