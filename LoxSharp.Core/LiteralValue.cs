using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoxSharp.Core
{
    public abstract class LiteralValue
    {
    }

    public class LiteralStringValue(string value) : LiteralValue
    {
        public string Value { get; } = value;

        public override string ToString() => $@"""{Value}""";
    }

    public class LiteralNumericValue(double value) : LiteralValue
    {
        public double Value { get; } = value;

        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
    }

    public class  LiteralNilValue : LiteralValue
    {
        public object? Value { get; } = null;

        public override string ToString() => "nil";
    }
}
