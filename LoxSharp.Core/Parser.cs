using System.Data;

namespace LoxSharp.Core
{
    public class Parser(IReadOnlyList<ILoxToken> tokens)
    {
        private readonly IReadOnlyList<ILoxToken> tokens = tokens;
        private int current;

        // -- Parser -- //

        public List<IStmt> Parse(out LoxError? error)
        {
            error = null;
            try
            {
                List<IStmt> statements = [];
                while (!IsAtEnd())
                {
                    statements.Add(Statement());
                }
                return statements;
            }
            catch (LoxParseException ex)
            {
                error = ex.Error;
                return [];
            }
        }

        private IStmt Statement()
        {
            if (Match(TokenType.Print))
                return PrintStatement();

            return ExpressionStatement();
        }

        private void Synchronize()
        {
            Advance();

            while (!IsAtEnd())
            {
                if (Previous().Type == TokenType.Semicolon)
                    return;

                switch (Peek().Type)
                {
                    case TokenType.Class:
                    case TokenType.Function:
                    case TokenType.Var:
                    case TokenType.For:
                    case TokenType.If:
                    case TokenType.While:
                    case TokenType.Print:
                    case TokenType.Return:
                        return;
                }
            }

            Advance();
        }

        private ExpressionStmt ExpressionStatement()
        {
            IExpr expr = Expression();
            Consume(TokenType.Semicolon, "Expect ';' after value.");
            return new ExpressionStmt(expr);
        }

        private PrintStmt PrintStatement()
        {
            IExpr expr = Expression();
            Consume(TokenType.Semicolon, "Expect ';' after expression.");
            return new PrintStmt(expr);
        }

        private IExpr Expression() => Equality();

        private IExpr Equality()
        {
            IExpr expr = Comparison();

            while (Match(TokenType.BangEqual, TokenType.EqualEqual))
            {
                ILoxToken op = Previous();
                IExpr right = Comparison();
                expr = new BinaryExpr(expr, op, right);
            }

            return expr;
        }

        private IExpr Comparison()
        {
            IExpr expr = Term();

            while (Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
            {
                ILoxToken op = Previous();
                IExpr right = Term();
                expr = new BinaryExpr(expr, op, right);
            }
            return expr;
        }

        private IExpr Term()
        {
            IExpr expr = Factor();

            while (Match(TokenType.Minus, TokenType.Plus))
            {
                ILoxToken op = Previous();
                IExpr right = Factor();
                expr = new BinaryExpr(expr, op, right);
            }
            return expr;
        }

        private IExpr Factor()
        {
            IExpr expr = Unary();

            while (Match(TokenType.Star, TokenType.Slash))
            {
                ILoxToken op = Previous();
                IExpr right = Factor();
                expr = new BinaryExpr(expr, op, right);
            }
            return expr;
        }

        private IExpr Unary()
        {
            if (Match(TokenType.Bang, TokenType.Minus))
            {
                ILoxToken op = Previous();
                IExpr right = Unary();
                return new UnaryExpr(op, right);
            }

            return Primary();
        }

        private IExpr Primary()
        {
            if (Match(TokenType.False))
                return new LiteralExpr(false);
            if (Match(TokenType.True))
                return new LiteralExpr(true);
            if (Match(TokenType.Nil))
                return new LiteralExpr(new LiteralNilValue());

            if (Match(TokenType.Number))
            {
                if (Previous() is not LoxTokenNumeric token)
                    throw new Exception($"Found number token but ILoxToken is implemented as {Previous().Type}.");
                return new LiteralExpr(token.Literal);
            }

            if (Match(TokenType.String))
            {
                if (Previous() is not LoxTokenString token)
                    throw new Exception($"Found string token but ILoxToken is implemented as {Previous().Type}.");
                return new LiteralExpr(token.Literal);
            }

            if (Match(TokenType.LeftParenthesis))
            {
                IExpr expr = Expression();
                Consume(TokenType.RightParenthesis, "Expected ')' after expression.");
                return new GroupingExpr(expr);
            }

            throw new LoxParseException(Peek(), "Expected expression.");

            // Note: The exceptions for Number and String tokens are not expected parser failures due to not well-formed code,
            //       but the result of a mistake in the parser/scanner implementation. They will not show up to the end user.
        }

        // -- Helper -- //

        private bool Match(params TokenType[] tokens)
        {
            foreach (var token in tokens)
            {
                if (Check(token))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        private ILoxToken Advance()
        {
            if (!IsAtEnd())
                current++;
            return Previous();
        }

        private bool Check(TokenType token) => !IsAtEnd() && Peek().Type == token;

        private ILoxToken Peek() => tokens[current];

        private bool IsAtEnd() => Peek().Type == TokenType.Eof;

        private ILoxToken Previous() => tokens[current - 1];

        private ILoxToken Consume(TokenType token, string message)
        {
            if (Check(token))
                return Advance();

            throw new LoxParseException(Peek(), message);
        }
    }
}
