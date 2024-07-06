using System.Text;

namespace LoxSharp.Core
{
    public class AstPrinter : IExprVisitor<string>
    {
        private string Parenthesise(string name, params IExpr[] expressions)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('(').Append(name);
            foreach (var expression in expressions)
            {
                builder.Append(' ').Append(expression.Accept(this));
            }
            builder.Append(')');

            return builder.ToString();
        }

        public string Print(IExpr expression) => expression.Accept(this);

        public string Visit(BinaryExpr expr) => Parenthesise(expr.Op.Lexeme, expr.Left, expr.Right);

        public string Visit(GroupingExpr expr) => Parenthesise("group", expr.Expression);

        public string Visit(LiteralExpr expr) => expr.Value.ToString() ?? string.Empty;

        public string Visit(UnaryExpr expr) => Parenthesise(expr.Op.Lexeme, expr.Right);
    }
}
