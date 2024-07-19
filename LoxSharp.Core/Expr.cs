namespace LoxSharp.Core
{
    public interface IExpr
    {
        T Accept<T>(IExprVisitor<T> visitor);
    }

    public interface IExprVisitor<T>
    {
        T Visit(AssignExpr expr);
        T Visit(BinaryExpr expr);
        T Visit(CallExpr expr);
        T Visit(GetExpr expr);
        T Visit(GroupingExpr expr);
        T Visit(LiteralExpr expr);
        T Visit(LogicalExpr expr);
        T Visit(SetExpr expr);
        T Visit(ThisExpr expr);
        T Visit(UnaryExpr expr);
        T Visit(VariableExpr expr);
    }

    public class AssignExpr(ILoxToken name, IExpr value) : IExpr
    {
        public ILoxToken Name { get; } = name;
        public IExpr Value { get; } = value;

        public T Accept<T>(IExprVisitor<T> visitor) => visitor.Visit(this);
    }

    public class BinaryExpr(IExpr left, ILoxToken op, IExpr right) : IExpr
    {
        public IExpr Left { get; } = left;
        public ILoxToken Op { get; } = op;
        public IExpr Right { get; } = right;

        public T Accept<T>(IExprVisitor<T> visitor) => visitor.Visit(this);
    }

    public class CallExpr(IExpr callee, ILoxToken parent, List<IExpr>? arguments) : IExpr
    {
        public IExpr Callee { get; } = callee;
        public ILoxToken Parent { get; } = parent;
        public List<IExpr>? Arguments { get; } = arguments;

        public T Accept<T>(IExprVisitor<T> visitor) => visitor.Visit(this);
    }

    public class GetExpr(IExpr obj, ILoxToken name) : IExpr
    {
        public IExpr Obj { get; } = obj;
        public ILoxToken Name { get; } = name;

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

    public class LogicalExpr(IExpr left, ILoxToken op, IExpr right) : IExpr
    {
        public IExpr Left { get; } = left;
        public ILoxToken Op { get; } = op;
        public IExpr Right { get; } = right;

        public T Accept<T>(IExprVisitor<T> visitor) => visitor.Visit(this);
    }

    public class SetExpr(IExpr obj, ILoxToken name, IExpr value) : IExpr
    {
        public IExpr Obj { get; } = obj;
        public ILoxToken Name { get; } = name;
        public IExpr Value { get; } = value;

        public T Accept<T>(IExprVisitor<T> visitor) => visitor.Visit(this);
    }

    public class ThisExpr(ILoxToken keyword) : IExpr
    {
        public ILoxToken Keyword { get; } = keyword;

        public T Accept<T>(IExprVisitor<T> visitor) => visitor.Visit(this);
    }

    public class UnaryExpr(ILoxToken op, IExpr right) : IExpr
    {
        public ILoxToken Op { get; } = op;
        public IExpr Right { get; } = right;

        public T Accept<T>(IExprVisitor<T> visitor) => visitor.Visit(this);
    }

    public class VariableExpr(ILoxToken name) : IExpr
    {
        public ILoxToken Name { get; } = name;

        public T Accept<T>(IExprVisitor<T> visitor) => visitor.Visit(this);
    }
}
