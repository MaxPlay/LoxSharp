﻿using System.Data;

namespace LoxSharp.Core
{
    public class Parser(IReadOnlyList<ILoxToken> tokens)
    {
        private readonly IReadOnlyList<ILoxToken> tokens = tokens;
        private int current;

        public IReadOnlyList<LoxError> Errors => errors;
        private readonly List<LoxError> errors = [];

        // -- Parser -- //

        public List<IStmt> Parse()
        {
            try
            {
                List<IStmt> statements = [];
                while (!IsAtEnd())
                {
                    IStmt? statement = Declaration();
                    if (statement != null)
                        statements.Add(statement);
                }
                return statements;
            }
            catch (LoxParseException ex)
            {
                if (ex.Error != null)
                    errors.Add(ex.Error);
                else
                    throw;
                return [];
            }
        }

        private IStmt? Declaration()
        {
            try
            {
                if (Match(TokenType.Var))
                    return VarDeclaration();
                return Statement();
            }
            catch (LoxParseException ex)
            {
                if (ex.Error != null)
                    errors.Add(ex.Error);
                else
                    throw;

                Synchronize();
                return null;
            }
        }

        private VarStmt VarDeclaration()
        {
            ILoxToken name = Consume(TokenType.Identifier, "Expected a variable name.");

            IExpr initializer = Match(TokenType.Equal) ? Expression() : new LiteralExpr(new LiteralNilValue());
            Consume(TokenType.Semicolon, "Expected ';' after variable declaration.");

            return new VarStmt(name, initializer);
        }

        private IStmt Statement()
        {
            if (Match(TokenType.Print))
                return PrintStatement();
            if (Match(TokenType.LeftBrace))
                return new BlockStmt(Block());

            return ExpressionStatement();
        }

        private List<IStmt> Block()
        {
            List<IStmt> statements = [];
            while (!Check(TokenType.RightBrace) && !IsAtEnd())
            {
                IStmt? statement = Declaration();
                if (statement != null)
                    statements.Add(statement);
            }

            Consume(TokenType.RightBrace, "Expected '}' after block.");
            return statements;
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

        private IExpr Expression() => Assignment();

        private IExpr Assignment()
        {
            IExpr expr = Equality();

            if (Match(TokenType.Equal))
            {
                ILoxToken equals = Previous();
                IExpr value = Assignment();

                if (expr is VariableExpr variableExpression)
                {
                    ILoxToken name = variableExpression.Name;
                    return new AssignExpr(name, value);
                }

                throw new LoxParseException(equals, "Invalid assignment target.");
            }

            return expr;
        }

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

            if (Match(TokenType.Identifier))
                return new VariableExpr(Previous());

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
