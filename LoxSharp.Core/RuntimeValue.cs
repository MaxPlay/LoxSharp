

using System.Diagnostics;

namespace LoxSharp.Core
{
    public enum RuntimeValueType : byte
    {
        Nil,
        Boolean,
        Numeric,
        String,
        Function
    }

    public struct RuntimeValue
    {
        public RuntimeValueType Type { get; private set; }
        private double numericValue;
        private string? stringValue;
        private bool boolValue;
        private ILoxCallable? functionValue;

        public static implicit operator RuntimeValue(bool value) => new RuntimeValue() { Type = RuntimeValueType.Boolean, boolValue = value };
        public static implicit operator RuntimeValue(double value) => new RuntimeValue() { Type = RuntimeValueType.Numeric, numericValue = value };
        public static implicit operator RuntimeValue(string? value) => value == null ?
            new RuntimeValue() { Type = RuntimeValueType.Nil } :
            new RuntimeValue() { Type = RuntimeValueType.String, stringValue = value };
        public static RuntimeValue MakeFunctionPointer(ILoxCallable value) => new RuntimeValue() { Type = RuntimeValueType.Function, functionValue = value };

        public readonly bool BoolValue => Type switch
        {
            RuntimeValueType.Nil => false,
            RuntimeValueType.Boolean => boolValue,
            _ => true,
        };

        public readonly double NumericValue => Type switch
        {
            RuntimeValueType.Boolean => boolValue ? 0 : 1,
            RuntimeValueType.Numeric => numericValue,
            RuntimeValueType.String => stringValue.ToDouble(),
            _ => 0
        };

        public readonly string StringValue => Type switch
        {
            RuntimeValueType.String => stringValue ?? string.Empty,
            RuntimeValueType.Nil => "Nil",
            RuntimeValueType.Numeric => numericValue.ToString(),
            RuntimeValueType.Boolean => boolValue.ToString(),
            _ => string.Empty
        };

        public readonly bool IsNil => Type == RuntimeValueType.Nil;

        public readonly ILoxCallable? FunctionValue => Type switch
        {
            RuntimeValueType.Function => functionValue,
            _ => null
        };

        public override readonly string ToString() => $"{Type}: {StringValue}";
    }

    public static class Helpers
    {
        public static double ToDouble(this string? value) => double.TryParse(value, out double result) ? result : double.NaN;
    }
}
