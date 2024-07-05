using LoxSharp.Core;
using Microsoft.Extensions.Logging;

namespace LoxSharp
{
    public enum RuntimeResult
    {
        Ok,
        InputError,
        RuntimeError
    }

    public class Runtime(ILogger<Runtime> logger, Interpreter interpreter)
    {
        private readonly ILogger logger = logger;
        private readonly Interpreter interpreter = interpreter;

        public RuntimeResult RunFile(string path)
        {
            if (!File.Exists(path))
            {
                logger.LogError("Could not find {path}", path);
                return RuntimeResult.InputError;
            }

            string[] lines = File.ReadAllLines(path);
            return Run(lines);
        }

        private RuntimeResult Run(string[] source)
        {
            RuntimeResult runtimeResult = RuntimeResult.Ok;

            Scanner scanner = new Scanner(source);
            IReadOnlyList<ILoxToken> tokens = scanner.Tokenize(out List<LoxError> errors);
            Parser parse = new Parser(tokens);
            IExpr? expression = parse.Parse(out LoxError? parseError);

            if (parseError == null)
            {
                if (expression != null)
                {
                    string result = interpreter.Interpret(expression, out LoxError? runtimeError);
                    if (runtimeError == null)
                    {
                        logger.LogInformation("{result}", result);
                    }
                    else
                    {
                        runtimeResult = RuntimeResult.RuntimeError;
                        errors.Add(runtimeError);
                    }
                }
                else
                    throw new Exception("Parsing failed without error.");
            }
            else
                errors.Add(parseError);

            foreach (var error in errors)
            {
                logger.LogError("Error: [{line}] Error{where}: {message}", error.Line, error.Where, error.Message);
            }

            if (errors.Count > 0 && runtimeResult == RuntimeResult.Ok)
                runtimeResult = RuntimeResult.InputError;

            return runtimeResult;
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
