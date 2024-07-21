using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoxSharp.Core
{
    public class LoxEnvironment
    {
        public LoxEnvironment? Enclosing { get; }
        private readonly Dictionary<string, RuntimeValue> values = [];

        public LoxEnvironment()
        {
            Enclosing = null;
        }

        public LoxEnvironment(LoxEnvironment enclosing)
        {
            this.Enclosing = enclosing;
        }

        public void Define(ILoxCallable value)
        {
            values[value.Identifier] = RuntimeValue.MakeFunctionPointer(value);
        }

        public void Define(string name)
        {
            values[name] = RuntimeValue.NullValue;
        }

        public void Define(string name, ref RuntimeValue value)
        {
            values[name] = value;
        }

        public void Define(string name, LoxClass type)
        {
            values[name] = type;
        }

        public void Get(ILoxToken name, out RuntimeValue value)
        {
            if (!values.TryGetValue(name.Lexeme, out value))
            {
                if (Enclosing != null)
                {
                    Enclosing.Get(name, out value);
                    return;
                }
                throw new LoxRuntimeException(name, $"Undefined variable '{name.Lexeme}'.");
            }
        }

        public void GetAt(int depth, string name, out RuntimeValue value)
        {
            Ancestor(depth).values.TryGetValue(name, out value);
        }

        public void Assign(ILoxToken name, LoxClass loxClass)
        {
            RuntimeValue value = loxClass;
            Assign(name, ref value);
        }

        public void Assign(ILoxToken name, ref RuntimeValue value)
        {
            if (values.ContainsKey(name.Lexeme))
            {
                values[name.Lexeme] = value;
                return;
            }

            if (Enclosing != null)
            {
                Enclosing.Assign(name, ref value);
                return;
            }

            throw new LoxRuntimeException(name, $"Undefined variable '{name.Lexeme}'.");
        }

        public void AssignAt(int depth, ILoxToken name, RuntimeValue value)
        {
            Ancestor(depth).values[name.Lexeme] = value;
        }

        private LoxEnvironment Ancestor(int depth)
        {
            LoxEnvironment environment = this;
            for (int i = 0; i < depth; i++)
            {
                if (environment.Enclosing == null)
                    throw new ArgumentOutOfRangeException(nameof(depth), "LoxEnvironment depth too large.");
                environment = environment.Enclosing;
            }
            return environment;
        }
    }
}
