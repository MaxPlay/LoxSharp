
namespace LoxSharp.Core
{
    public class LoxFunction(FunctionStmt declaration, LoxEnvironment closure, bool isInitializer) : ILoxCallable
    {
        public const string INITIALIZER_KEYWORD = "init";

        private readonly FunctionStmt declaration = declaration;
        private readonly LoxEnvironment closure = closure;
        private readonly bool isInitializer = isInitializer;

        public string Identifier => declaration.Name.Lexeme;
        public int Arity => declaration.Parameters.Count;

        public LoxFunction Bind(LoxInstance loxInstance)
        {
            LoxEnvironment environment = new LoxEnvironment(closure);
            RuntimeValue thisValue = loxInstance;
            environment.Define("this", ref thisValue);
            return new LoxFunction(declaration, environment, isInitializer);
        }

        public RuntimeValue Call(Interpreter interpreter, List<RuntimeValue>? arguments)
        {
            LoxEnvironment environment = new LoxEnvironment(closure);
            if (arguments != null && declaration.Parameters.Count > 0)
            {
                for (int i = 0; i < declaration.Parameters.Count; i++)
                {
                    RuntimeValue value = arguments[i];
                    environment.Define(declaration.Parameters[i].Lexeme, ref value);
                }
            }

            try
            {
                interpreter.ExecuteBlock(declaration.Body, environment);
            }
            catch (ReturnException returnValue)
            {
                if (isInitializer)
                {
                    closure.GetAt(0, "this", out RuntimeValue thisValue);
                    return thisValue;
                }
                return returnValue.Value ?? RuntimeValue.NullValue;
            }
            return RuntimeValue.NullValue;
        }

        public override string ToString() => $"<fn {declaration.Name.Lexeme}>";
    }
}
