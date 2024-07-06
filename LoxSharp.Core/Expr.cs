namespace LoxSharp.Core
{
    public interface IExpr
    {
        T Accept<T>(IExprVisitor<T> visitor);
    }

    public interface IExprVisitor<T>
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

        public T Accept<T>(IExprVisitor<T> visitor) => visitor.Visit(this);
    }

    public class GroupingExpr(IExpr expression) : IExpr
    {
        public IExpr Expression { get; } = expression;

        public T Accept<T>(IExprVisitor<T> visitor) => visitor.Visit(this);
    }

    public class LiteralExpr(LiteralValue value) : IExpr
    {
        public LiteralValue Value { get; } = value;

        public T Accept<T>(IExprVisitor<T> visitor) => visitor.Visit(this);
    }

    public class UnaryExpr(ILoxToken op, IExpr right) : IExpr
    {
        public ILoxToken Op { get; } = op;
        public IExpr Right { get; } = right;

        public T Accept<T>(IExprVisitor<T> visitor) => visitor.Visit(this);
    }
}
