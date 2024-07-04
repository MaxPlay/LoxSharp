using LoxSharp.Core;
using Microsoft.Extensions.Logging;

namespace LoxSharp
{
    public class Runtime(ILogger<Runtime> logger, AstPrinter astPrinter)
    {
        private readonly ILogger logger = logger;
        private readonly AstPrinter astPrinter = astPrinter;

        public bool RunFile(string path)
        {
            if (!File.Exists(path))
            {
                logger.LogError("Could not find {path}", path);
                return false;
            }

            string[] lines = File.ReadAllLines(path);
            return Run(lines);
        }

        private bool Run(string[] source)
        {
            Scanner scanner = new Scanner(source);
            IReadOnlyList<ILoxToken> tokens = scanner.Tokenize(out List<LoxError> errors);
            Parser parse = new Parser(tokens);
            IExpr? expression = parse.Parse(out LoxError? parseError);

            if (parseError != null)
                errors.Add(parseError);

            if (expression != null)
                logger.LogDebug("{ast}",astPrinter.Print(expression));

            foreach (var error in errors)
            {
                logger.LogError("Error: [{line}] Error{where}: {message}", error.Line, error.Where, error.Message);
            }
            return errors.Count == 0;
        }

        public void RunPrompt()
        {
            string? line;
            string[] lines = new string[1];
            while (true)
            {
                Console.Write("> ");
                line = Console.ReadLine();
                if (line == null)
                    break;

                lines[0] = line;
                Run(lines);
            }
        }
    }
}
