using LoxSharp.Core;
using Microsoft.Extensions.Logging;

namespace LoxSharp
{
    public class Runtime(ILogger<Runtime> logger)
    {
        readonly ILogger logger = logger;

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
            IReadOnlyList<ILoxToken> tokens = scanner.Tokenize(out List<ScanError> errors);

            foreach (var token in tokens)
            {
                logger.LogDebug("{token}", token);
            }

            foreach (var error in errors)
            {
                logger.LogError("Error: [{line}] Error: {message}", error.Line, error.Message);
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
