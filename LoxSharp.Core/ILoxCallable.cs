namespace LoxSharp.Core
{
    public interface ILoxCallable
    {
        int Arity { get; }

        RuntimeValue Call(Interpreter interpreter, List<RuntimeValue>? arguments);
    }

    public abstract class LoxCallable : ILoxCallable
    {
        public abstract int Arity { get; }

        public static LoxCallable MakeNative(Func<RuntimeValue> func) => new NativeLoxCallable_Param0(func);
        public static LoxCallable MakeNative(Func<RuntimeValue, RuntimeValue> func) => new NativeLoxCallable_Param1(func);

        public abstract RuntimeValue Call(Interpreter interpreter, List<RuntimeValue>? arguments);
    }

    public class NativeLoxCallable_Param0(Func<RuntimeValue> func) : LoxCallable
    {
        public override int Arity => 0;

        private readonly Func<RuntimeValue> func = func;

        public override RuntimeValue Call(Interpreter interpreter, List<RuntimeValue>? arguments) => func.Invoke();
    }
    public class NativeLoxCallable_Param1(Func<RuntimeValue, RuntimeValue> func) : LoxCallable
    {
        public override int Arity => 0;

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
