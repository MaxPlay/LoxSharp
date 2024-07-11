namespace LoxSharp.Core
{
    public interface ILoxCallable
    {
        string Identifier { get; }
        int Arity { get; }

        RuntimeValue Call(Interpreter interpreter, List<RuntimeValue>? arguments);
    }

    public abstract class NativeLoxCallable : ILoxCallable
    {
        public abstract string Identifier { get; }
        public abstract int Arity { get; }

        public static ILoxCallable Make(string identifier, Func<RuntimeValue> func) => new NativeLoxCallable_Param0(identifier, func);
        public static ILoxCallable Make(string identifier, Func<RuntimeValue, RuntimeValue> func) => new NativeLoxCallable_Param1(identifier, func);

        public abstract RuntimeValue Call(Interpreter interpreter, List<RuntimeValue>? arguments);
        public override string ToString() => $"<native fn {Identifier}>";
    }

    public class NativeLoxCallable_Param0(string identifier, Func<RuntimeValue> func) : NativeLoxCallable
    {
        public override string Identifier { get; } = identifier;
        public override int Arity => 0;

        private readonly Func<RuntimeValue> func = func;

        public override RuntimeValue Call(Interpreter interpreter, List<RuntimeValue>? arguments) => func.Invoke();
    }
    public class NativeLoxCallable_Param1(string identifier, Func<RuntimeValue, RuntimeValue> func) : NativeLoxCallable
    {
        public override string Identifier { get; } = identifier;

        public override int Arity => 1;

        private readonly Func<RuntimeValue, RuntimeValue> func = func;

        public override RuntimeValue Call(Interpreter interpreter, List<RuntimeValue>? arguments)
        {
            if (arguments?.Count > 0)
            {
                return func.Invoke(arguments[0]);
            }
            return null;
        }
    }
}
