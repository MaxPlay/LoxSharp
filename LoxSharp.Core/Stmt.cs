namespace LoxSharp.Core
{
    public interface IStmt
    {
        T Accept<T>(IStmtVisitor<T> visitor);
    }

    public interface IStmtVisitor<T>
    {
        T Visit(ExpressionStmt stmt);
        T Visit(PrintStmt stmt);
    }

    public class ExpressionStmt(IExpr expression) : IStmt
    {
        public IExpr Expression { get; } = expression;

        public T Accept<T>(IStmtVisitor<T> visitor) => visitor.Visit(this);
    }

    public class PrintStmt(IExpr expression) : IStmt
    {
        public IExpr Expression { get; } = expression;

        public T Accept<T>(IStmtVisitor<T> visitor) => visitor.Visit(this);
    }
}
