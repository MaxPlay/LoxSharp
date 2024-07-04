using System.Reflection.Emit;

namespace LoxSharp.Core
{
    public interface IExpr
    {
        T Accept<T>(IVisitor<T> visitor);
    }

    public interface IVisitor<T>
    {
        T Visit(BinaryExpr expr);
        T Visit(GroupingExpr expr);
        T Visit(LiteralExpr expr);
        T Visit(UnaryExpr expr);
    }

    public class BinaryExpr(IExpr left, ILoxToken op, IExpr right) : IExpr
    {
        public IExpr Left { get; } = left;
        public ILoxToken Op { get; } = op;
        public IExpr Right { get; } = right;

        public T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }

    public class GroupingExpr(IExpr expression) : IExpr
    {
        public IExpr Expression { get; } = expression;

        public T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }

    public class LiteralExpr : IExpr
    {
        public LiteralExpr(LiteralValue value) => Value = value;

        public LiteralExpr(bool value) => Value = new LiteralBoolValue(value);

        public LiteralExpr(string value) => Value = new LiteralStringValue(value);

        public LiteralExpr() => Value = new LiteralNilValue();

        public LiteralExpr(double value) => Value = new LiteralNumericValue(value);

        public LiteralValue Value { get; }

        public T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }

    public class UnaryExpr(ILoxToken op, IExpr right) : IExpr
    {
        public ILoxToken Op { get; } = op;
        public IExpr Right { get; } = right;

        public T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }
}
