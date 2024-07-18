namespace LoxSharp.Core
{
    public class LoxFunction(FunctionStmt declaration, LoxEnvironment closure) : ILoxCallable
    {
        private readonly FunctionStmt declaration = declaration;
        private readonly LoxEnvironment closure = closure;

        public string Identifier => declaration.Name.Lexeme;
        public int Arity => declaration.Parameters.Count;

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
            catch(ReturnException returnValue)
            {
                return returnValue.Value ?? RuntimeValue.NullValue;
            }
            return RuntimeValue.NullValue;
        }

        public override string ToString() => $"<fn {declaration.Name.Lexeme}>";
    }
}
