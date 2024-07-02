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

    public class BinaryExpr(IExpr left, TokenType op, IExpr right) : IExpr
    {
        private readonly IExpr left = left;
        private readonly TokenType op = op;
        private readonly IExpr right = right;

        public T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }

    public class GroupingExpr(IExpr expression) : IExpr
    {
        private readonly IExpr expression = expression;

        public T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }

    public class LiteralExpr(LiteralValue value) : IExpr
    {
        private readonly LiteralValue value = value;

        public T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }

    public class UnaryExpr(TokenType op, IExpr right) : IExpr
    {
        private readonly TokenType op = op;
        private readonly IExpr right = right;

        public T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }
}
