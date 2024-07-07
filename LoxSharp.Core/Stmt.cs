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
        T Visit(IfStmt stmt);
        T Visit(PrintStmt stmt);
        T Visit(VarStmt stmt);
        T Visit(WhileStmt stmt);
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

    public class IfStmt(IExpr condition, IStmt thenBranch, IStmt? elseBranch) : IStmt
    {
        public IExpr Condition { get; } = condition;
        public IStmt ThenBranch { get; } = thenBranch;
        public IStmt? ElseBranch { get; } = elseBranch;

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

    public class WhileStmt(IExpr condition, IStmt body) : IStmt
    {
        public IExpr Condition { get; } = condition;
        public IStmt Body { get; } = body;

        public T Accept<T>(IStmtVisitor<T> visitor) => visitor.Visit(this);
    }
}
