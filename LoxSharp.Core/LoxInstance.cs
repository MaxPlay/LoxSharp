

namespace LoxSharp.Core
{
    public class LoxInstance(LoxClass loxClass)
    {
        private readonly LoxClass loxClass = loxClass;
        private readonly Dictionary<string, RuntimeValue> fields = [];

        public override string ToString() => $"{loxClass.Identifier} instance";

        public void Get(ILoxToken name, out RuntimeValue value)
        {
            if (fields.TryGetValue(name.Lexeme, out value))
                return;

            LoxFunction? method = loxClass.FindMethod(name.Lexeme);
            if (method != null)
            {
                value = RuntimeValue.MakeFunctionPointer(method.Bind(this));
                return;
            }

            throw new LoxRuntimeException(name, $"Undefined property '{name.Lexeme}'.");
        }

        public void Set(ILoxToken name, ref RuntimeValue value)
        {
            fields[name.Lexeme] = value;
        }
    }
}