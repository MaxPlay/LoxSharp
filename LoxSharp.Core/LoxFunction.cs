namespace LoxSharp.Core
{
    public class LoxFunction(FunctionStmt declaration) : ILoxCallable
    {
        private readonly FunctionStmt declaration = declaration;

        public string Identifier => declaration.Name.Lexeme;
        public int Arity => declaration.Parameters.Count;

        public RuntimeValue Call(Interpreter interpreter, List<RuntimeValue>? arguments)
        {
            LoxEnvironment environment = new LoxEnvironment(interpreter.Globals);
            if (arguments != null && declaration.Parameters.Count > 0)
            {
                for (int i = 0; i < declaration.Parameters.Count; i++)
                {
                    RuntimeValue value = arguments[i];
                    environment.Define(declaration.Parameters[i].Lexeme, ref value);
                }
            }

            interpreter.ExecuteBlock(declaration.Body, environment);
            return null;
        }

        public override string ToString() => $"<fn {declaration.Name.Lexeme}>";
    }
}
