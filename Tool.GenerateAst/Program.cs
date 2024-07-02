namespace Tool.GenerateAst
{
    internal class Program
    {
        static int Main(string[] args)
        {
            Application application = new Application(args);
            return application.Execute();
        }
    }
}


//namespace LoxSharp.Core
//{
//    public struct Token { }
//    public struct LiteralValue { }

//    public interface IExpr
//    {
//        T Accept<T>(IVisitor<T> visitor);
//    }

//    public interface IVisitor<T>
//    {
//        T Visit(BinaryExpr expr);
//        T Visit(UnaryExpr expr);
//        T Visit(LiteralExpr expr);
//        T Visit(GroupingExpr expr);
//    }

//    public class BinaryExpr(IExpr left, Token op, IExpr right) : IExpr
//    {
//        private readonly IExpr left = left;
//        private readonly Token op = op;
//        private readonly IExpr right = right;

//        public R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
//    }
//    public class UnaryExpr(Token op, IExpr right) : IExpr
//    {
//        private readonly Token op = op;
//        private readonly IExpr right = right;

//        public R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
//    }
//    public class GroupingExpr(IExpr expression) : IExpr
//    {
//        private readonly IExpr expression = expression;

//        public R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
//    }
//    public class LiteralExpr(LiteralValue value) : IExpr
//    {
//        private readonly LiteralValue value = value;

//        public R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
//    }
//}