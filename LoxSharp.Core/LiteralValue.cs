using System.Globalization;

namespace LoxSharp.Core
{
    public abstract class LiteralValue
    {
        public static implicit operator LiteralValue(bool value) => new LiteralBoolValue(value);
        public static implicit operator LiteralValue(double value) => new LiteralNumericValue(value);
        public static implicit operator LiteralValue(string value) => new LiteralStringValue(value);
    }

    public class LiteralBoolValue(bool value) : LiteralValue
    {
        public bool Value { get; } = value;

        public override string ToString() => Value.ToString();
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

    public class LiteralNilValue : LiteralValue
    {
        public object? Value { get; } = null;

        public override string ToString() => "nil";
    }
}
