
namespace LoxSharp.Core
{
    public class LoxClass(string name) : ILoxCallable
    {
        private readonly string name = name;

        public string Identifier => name;
        public int Arity => 0;

        public RuntimeValue Call(Interpreter interpreter, List<RuntimeValue>? arguments)
        {
            LoxInstance instance = new LoxInstance(this);
            return instance;
        }

        public override string ToString() => name;
    }
}