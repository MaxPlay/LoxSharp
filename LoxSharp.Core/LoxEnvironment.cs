using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoxSharp.Core
{
    public class LoxEnvironment
    {
        private readonly LoxEnvironment? enclosing;
        private readonly Dictionary<string, RuntimeValue> values = [];

        public LoxEnvironment()
        {
            enclosing = null;
        }

        public LoxEnvironment(LoxEnvironment enclosing)
        {
            this.enclosing = enclosing;
        }

        public void Define(string name, ILoxCallable value)
        {
            values[name] = RuntimeValue.MakeFunctionPointer(value);
        }

        public void Define(string name, ref RuntimeValue value)
        {
            values[name] = value;
        }

        public void Get(ILoxToken name, out RuntimeValue value)
        {
            if (!values.TryGetValue(name.Lexeme, out value))
            {
                if (enclosing != null)
                {
                    enclosing.Get(name, out value);
                    return;
                }
                throw new LoxRuntimeException(name, $"Undefined variable '{name.Lexeme}'.");
            }
        }

        public void Assign(ILoxToken name, ref RuntimeValue value)
        {
            if (values.ContainsKey(name.Lexeme))
            {
                values[name.Lexeme] = value;
                return;
            }

            if (enclosing != null)
            {
                enclosing.Assign(name, ref value);
                return;
            }

            throw new LoxRuntimeException(name, $"Undefined variable '{name.Lexeme}'.");
        }
    }
}
