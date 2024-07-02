using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoxSharp.Core
{
    public abstract class LiteralValue
    {
    }

    public class LiteralStringValue : LiteralValue
    {
        public string Value { get; set; } = string.Empty;

        public override string ToString() => $@"""{Value}""";
    }

    public class LiteralNumericValue : LiteralValue
    {
        public double Value { get; set; }

        public override string ToString() => Value.ToString();
    }

    public class  LiteralNilValue : LiteralValue
    {
        public object? Value { get; } = null;

        public override string ToString() => "nil";
    }
}
