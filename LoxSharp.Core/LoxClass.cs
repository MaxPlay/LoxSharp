
namespace LoxSharp.Core
{
    public class LoxClass : ILoxCallable
    {
        private readonly string name;
        private readonly Dictionary<string, LoxFunction> methods;

        public LoxClass(string name, Dictionary<string, LoxFunction> methods)
        {
            this.name = name;
            this.methods = methods;

            LoxFunction? initializer = FindMethod(LoxFunction.INITIALIZER_KEYWORD);
            Arity = initializer?.Arity ?? 0;
        }

        public string Identifier => name;
        public int Arity { get; }

        public RuntimeValue Call(Interpreter interpreter, List<RuntimeValue>? arguments)
        {
            LoxInstance instance = new LoxInstance(this);
            // Constructor call
            FindMethod(LoxFunction.INITIALIZER_KEYWORD)?.Bind(instance).Call(interpreter, arguments);
            return instance;
        }

        public LoxFunction? FindMethod(string name)
        {
            methods.TryGetValue(name, out LoxFunction? method);
            return method;
        }

        public override string ToString() => name;
    }
}