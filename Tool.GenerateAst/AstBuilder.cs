
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Tool.GenerateAst
{
    public class AstBuilder
    {
        private class AstType
        {
            public string Name { get; set; } = string.Empty;
            public List<AstMember> Members { get; set; } = [];

            public override string ToString() => Name;
        }

        private class AstMember
        {
            public string Name { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;

            public override string ToString() => $"{Type} {Name}";

            public string ToPropertyString()
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(Type);
                builder.Append(' ');
                if (Name.Length > 0)
                    builder.Append(char.ToUpperInvariant(Name[0]));
                if (Name.Length > 1)
                    builder.Append(Name[1..]);
                return builder.ToString();
            }
        }

        private class ScopeWriter : IDisposable
        {
            private static int scopeDepth;
            private readonly TextWriter writer;

            public ScopeWriter(TextWriter writer)
            {
                this.writer = writer;

                WriteScope();
                writer.WriteLine("{");
                scopeDepth++;
            }

            private void WriteScope()
            {
                for (int i = 0; i < scopeDepth; i++)
                {
                    writer.Write("    ");
                }
            }

            public void WriteLine(string value)
            {
                WriteScope();
                writer.WriteLine(value);
            }

            public void Dispose()
            {
                scopeDepth--;
                WriteScope();
                writer.WriteLine("}");
            }
        }

        private readonly ILogger logger;
        private readonly TreeConfiguration configuration;
        private readonly Dictionary<string, Domain> domains = [];

        class Domain
        {
            public List<AstType> Types { get; set; } = [];
            public string Interface { get; set; } = string.Empty;
            public string Visitor { get; set; } = string.Empty;
            public string ParameterIdentifier { get; set; } = string.Empty;
        }

        public AstBuilder(ILogger<AstBuilder> logger, TreeConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;

            foreach (var domain in configuration.Domains.Keys)
            {
                LoadTypeConfiguration(domain);
            }
        }

        private void LoadTypeConfiguration(string domainIdentifier)
        {
            if (!configuration.Domains.TryGetValue(domainIdentifier, out TreeConfigurationDomain? domainConfiguration))
                throw new Exception(@$"Domain ""{domainIdentifier}"" not found in configuration");

            if (!domains.TryGetValue(domainIdentifier, out Domain? domain))
            {
                domain = new Domain
                {
                    Visitor = domainConfiguration.Visitor,
                    Interface = GetRealTypeIdentifier(domainConfiguration.Interface),
                    ParameterIdentifier = domainConfiguration.ParameterIdentifier,
                };
                domains.Add(domainIdentifier, domain);
            }

            domain.Types.Clear();
            foreach (var definition in domainConfiguration.Definitions)
            {
                string[] typeDef = definition.Split(':');
                if (typeDef.Length != 2)
                {
                    logger.LogError("Type definition is invalid and will be skipped: {definition}", definition);
                    continue;
                }

                AstType type = new AstType()
                {
                    Name = typeDef[0] + domainIdentifier,
                };

                string[] members = typeDef[1].Split(',');
                foreach (var member in members)
                {
                    string[] memberDef = member.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (typeDef.Length != 2)
                    {
                        logger.LogError("Member definition is invalid and will be skipped: {member}", member);
                        continue;
                    }
                    string memberType = GetRealTypeIdentifier(memberDef[0]);

                    type.Members.Add(new AstMember { Name = memberDef[1], Type = memberType });
                }

                domain.Types.Add(type);
            }

            string GetRealTypeIdentifier(string placeholder)
            {
                return placeholder switch
                {
                    "expression" => configuration.Expression,
                    "statement" => configuration.Statement,
                    "token" => configuration.Token,
                    "visitor" => domainConfiguration.Visitor,
                    "literal" => configuration.Literal,
                    _ => throw new Exception("Invalid memberType.")
                };
            }
        }

        public bool BuildTree(string directoryPath)
        {
            foreach (var domain in configuration.Domains.Keys)
            {
                if (!BuildTree(domain, Path.Combine(directoryPath, domain + ".cs")))
                    return false;
            }
            return true;
        }

        public bool BuildTree(string domainIdentifier, string filePath)
        {
            if (!domains.TryGetValue(domainIdentifier, out Domain? domain))
                throw new Exception(@$"Domain ""{domainIdentifier}"" not found.");

            string? parentDirectory = Path.GetDirectoryName(filePath);
            if (parentDirectory != null)
            {
                Directory.CreateDirectory(parentDirectory);
            }

            string? name = Path.GetFileName(filePath);
            if (name == null)
            {
                logger.LogError("No valid filename specified: {name}", name);
                return false;
            }

            using FileStream stream = File.Open(filePath, FileMode.Create);
            using StreamWriter writer = new StreamWriter(stream);

            {
                writer.WriteLine($"namespace {configuration.Namespace}");
                using ScopeWriter namespaceWriter = new ScopeWriter(writer);
                {
                    namespaceWriter.WriteLine($"public interface {domain.Interface}");
                    using ScopeWriter interfaceWriter = new ScopeWriter(writer);
                    interfaceWriter.WriteLine($"T Accept<T>({domain.Visitor}<T> visitor);");
                }
                writer.WriteLine();
                {
                    namespaceWriter.WriteLine($"public interface {domain.Visitor}<T>");
                    using ScopeWriter interfaceWriter = new ScopeWriter(writer);
                    foreach (var type in domain.Types)
                    {
                        interfaceWriter.WriteLine($"T Visit({type} {domain.ParameterIdentifier});");
                    }
                }
                foreach (var type in domain.Types)
                {
                    writer.WriteLine();
                    namespaceWriter.WriteLine($"public class {type}({string.Join(", ", type.Members)}) : {domain.Interface}");
                    using ScopeWriter classWriter = new ScopeWriter(writer);
                    foreach (var member in type.Members)
                    {
                        classWriter.WriteLine($"public {member.ToPropertyString()} {{ get; }} = {member.Name};");
                    }
                    writer.WriteLine();
                    classWriter.WriteLine($"public T Accept<T>({domain.Visitor}<T> visitor) => visitor.Visit(this);");
                }
            }
            return true;
        }
    }
}
