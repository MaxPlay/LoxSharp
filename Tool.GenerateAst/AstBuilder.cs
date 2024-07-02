
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;

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
        private readonly List<AstType> types = [];

        public AstBuilder(ILogger<AstBuilder> logger, TreeConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;

            LoadTypeConfiguration();
        }

        private void LoadTypeConfiguration()
        {
            types.Clear();
            foreach (var definition in configuration.Definitions)
            {
                string[] typeDef = definition.Split(':');
                if (typeDef.Length != 2)
                {
                    logger.LogError("Type definition is invalid and will be skipped: {definition}", definition);
                    continue;
                }

                AstType type = new AstType()
                {
                    Name = typeDef[0] + "Expr",
                };

                string[] members = typeDef[1].Split(',');
                foreach (var member in members)
                {
                    string[] memberDef = member.Split(' ',StringSplitOptions.RemoveEmptyEntries);
                    if (typeDef.Length != 2)
                    {
                        logger.LogError("Member definition is invalid and will be skipped: {member}", member);
                        continue;
                    }

                    string memberType = memberDef[0] switch
                    {
                        "expression" => configuration.Expression,
                        "token" => configuration.Token,
                        "visitor" => configuration.Visitor,
                        "literal" => configuration.Literal,
                        _ => throw new Exception("Invalid memberType.")
                    };

                    type.Members.Add(new AstMember { Name = memberDef[1], Type = memberType });
                }

                types.Add(type);
            }
        }

        public bool BuildTree(string filePath)
        {
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
                    namespaceWriter.WriteLine($"public interface {configuration.Expression}");
                    using ScopeWriter interfaceWriter = new ScopeWriter(writer);
                    interfaceWriter.WriteLine($"T Accept<T>({configuration.Visitor}<T> visitor);");
                }
                writer.WriteLine();
                {
                    namespaceWriter.WriteLine($"public interface {configuration.Visitor}<T>");
                    using ScopeWriter interfaceWriter = new ScopeWriter(writer);
                    foreach (var type in types)
                    {
                        interfaceWriter.WriteLine($"T Visit({type} expr);");
                    }
                }
                foreach (var type in types)
                {
                    writer.WriteLine();
                    namespaceWriter.WriteLine($"public class {type}({string.Join(", ", type.Members)}) : {configuration.Expression}");
                    using ScopeWriter classWriter = new ScopeWriter(writer);
                    foreach (var member in type.Members)
                    {
                        classWriter.WriteLine($"private readonly {member} = {member.Name};");
                    }
                    writer.WriteLine();
                    classWriter.WriteLine($"public T Accept<T>({configuration.Visitor}<T> visitor) => visitor.Visit(this);");
                }
            }
            return true;
        }
    }
}
