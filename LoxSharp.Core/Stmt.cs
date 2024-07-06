namespace LoxSharp.Core
{
    public interface IStmt
    {
        T Accept<T>(IStmtVisitor<T> visitor);
    }

    public interface IStmtVisitor<T>
    {
        T Visit(BlockStmt stmt);
        T Visit(ExpressionStmt stmt);
        T Visit(PrintStmt stmt);
        T Visit(VarStmt stmt);
    }

    public class BlockStmt(List<IStmt> statements) : IStmt
    {
        public List<IStmt> Statements { get; } = statements;

        public T Accept<T>(IStmtVisitor<T> visitor) => visitor.Visit(this);
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

    public class VarStmt(ILoxToken name, IExpr initializer) : IStmt
    {
        public ILoxToken Name { get; } = name;
        public IExpr Initializer { get; } = initializer;

        public T Accept<T>(IStmtVisitor<T> visitor) => visitor.Visit(this);
    }
}
