namespace LoxSharp.Core
{
    public class Parser(IReadOnlyList<ILoxToken> tokens)
    {
        private enum FunctionType
        {
            Function,
            Method
        }

        private const int MAX_FUNCTION_PARAMETERS = 255;
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
                if (Match(TokenType.Class))
                    return ClassDeclaration();
                if (Match(TokenType.Function))
                    return FunctionDeclaration(FunctionType.Function);
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

        private ClassStmt ClassDeclaration()
        {
            ILoxToken name = Consume(TokenType.Identifier, "Expected class name.");

            VariableExpr? superclass = null;
            if (Match(TokenType.Less))
            {
                Consume(TokenType.Identifier, "Expected superclass name.");
                superclass = new VariableExpr(Previous());
            }

            Consume(TokenType.LeftBrace, "Expected '{' before class body.");

            List<FunctionStmt> methods = [];
            while (!Check(TokenType.RightBrace) && !IsAtEnd())
            {
                methods.Add(FunctionDeclaration(FunctionType.Method));
            }

            Consume(TokenType.RightBrace, "Expected '}' after class body.");

            return new ClassStmt(name, superclass, methods);
        }

        private FunctionStmt FunctionDeclaration(FunctionType type)
        {
            ILoxToken token = Consume(TokenType.Identifier, $"Expected {type} name.");
            Consume(TokenType.LeftParenthesis, $"Expected '(' after {type} name.");
            List<ILoxToken> parameters = [];
            if (!Check(TokenType.RightParenthesis))
            {
                do
                {
                    if (parameters.Count >= MAX_FUNCTION_PARAMETERS)
                        AddError($"Can't have more than {MAX_FUNCTION_PARAMETERS} parameters.");

                    parameters.Add(Consume(TokenType.Identifier, "Expected parameter name."));
                } while (Match(TokenType.Comma));
            }

            Consume(TokenType.RightParenthesis, "Expected ')' after parameters.");
            Consume(TokenType.LeftBrace, "Expected '{' after parameters.");
            List<IStmt> body = Block();
            return new FunctionStmt(token, parameters, body);
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
            if (Match(TokenType.For))
                return For();
            if (Match(TokenType.If))
                return If();
            if (Match(TokenType.Print))
                return Print();
            if (Match(TokenType.Return))
                return Return();
            if (Match(TokenType.While))
                return While();
            if (Match(TokenType.LeftBrace))
                return new BlockStmt(Block());

            return ExpressionStatement();
        }

        private ReturnStmt Return()
        {
            ILoxToken keyword = Previous();
            IExpr? value = null;
            if (!Check(TokenType.Semicolon))
                value = Expression();

            Consume(TokenType.Semicolon, "Expected ';' after return statement.");
            return new ReturnStmt(keyword, value);
        }

        private IStmt For()
        {
            Consume(TokenType.LeftParenthesis, "Expected '(' after 'for'.");
            IStmt? initializer;
            if (Match(TokenType.Semicolon))
                initializer = null;
            else if (Match(TokenType.Var))
                initializer = VarDeclaration();
            else
                initializer = ExpressionStatement();

            IExpr? condition = null;
            if (!Check(TokenType.Semicolon))
                condition = Expression();
            Consume(TokenType.Semicolon, "Expected ';' after loop condition.");

            IExpr? increment = null;
            if (!Check(TokenType.RightParenthesis))
                increment = Expression();
            Consume(TokenType.RightParenthesis, "Expected ')' after for clauses.");

            IStmt body = Statement();

            if (increment != null)
                body = new BlockStmt([body, new ExpressionStmt(increment)]);

            condition ??= new LiteralExpr(true);

            body = new WhileStmt(condition, body);

            if (initializer != null)
                body = new BlockStmt([initializer, body]);

            return body;
        }

        private WhileStmt While()
        {
            Consume(TokenType.LeftParenthesis, "Expected '(' after 'while'.");
            IExpr condition = Expression();
            Consume(TokenType.RightParenthesis, "Expected ')' after condition.");
            IStmt body = Statement();

            return new WhileStmt(condition, body);
        }

        private IfStmt If()
        {
            Consume(TokenType.LeftParenthesis, "Expected '(' after 'if'.");
            IExpr condition = Expression();
            Consume(TokenType.RightParenthesis, "Expected ')' after condition.");

            IStmt thenBranch = Statement();
            IStmt? elseBranch = null;
            if (Match(TokenType.Else))
                elseBranch = Statement();
            return new IfStmt(condition, thenBranch, elseBranch);
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

        private PrintStmt Print()
        {
            IExpr expr = Expression();
            Consume(TokenType.Semicolon, "Expect ';' after expression.");
            return new PrintStmt(expr);
        }

        private IExpr Expression() => Assignment();

        private IExpr Assignment()
        {
            IExpr expr = Or();

            if (Match(TokenType.Equal))
            {
                ILoxToken equals = Previous();
                IExpr value = Assignment();

                if (expr is VariableExpr variableExpression)
                {
                    ILoxToken name = variableExpression.Name;
                    return new AssignExpr(name, value);
                }
                else if (expr is GetExpr get)
                {
                    return new SetExpr(get.Obj, get.Name, value);
                }

                AddError(equals, "Invalid assignment target.");
            }

            return expr;
        }

        private IExpr Or()
        {
            IExpr expr = And();

            while (Match(TokenType.Or))
            {
                ILoxToken op = Previous();
                IExpr right = And();
                expr = new LogicalExpr(expr, op, right);
            }

            return expr;
        }

        private IExpr And()
        {
            IExpr expr = Equality();

            while (Match(TokenType.And))
            {
                ILoxToken op = Previous();
                IExpr right = Equality();
                expr = new LogicalExpr(expr, op, right);
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

            return Call();
        }

        private IExpr Call()
        {
            IExpr expr = Primary();

            while (true)
            {
                if (Match(TokenType.LeftParenthesis))
                {
                    expr = FinishCall(expr);
                }
                else if (Match(TokenType.Dot))
                {
                    ILoxToken name = Consume(TokenType.Identifier, "Expected property name after '.'.");
                    expr = new GetExpr(expr, name);
                }
                else
                    break;
            }

            return expr;
        }

        private CallExpr FinishCall(IExpr callee)
        {
            List<IExpr> arguments = [];
            if (!Check(TokenType.RightParenthesis))
            {
                do
                {
                    if (arguments.Count >= MAX_FUNCTION_PARAMETERS)
                        AddError($"Can't have more than {MAX_FUNCTION_PARAMETERS} arguments.");
                    arguments.Add(Expression());
                } while (Match(TokenType.Comma));
            }

            ILoxToken parent = Consume(TokenType.RightParenthesis, "Expected ')' after arguments.");

            return new CallExpr(callee, parent, arguments.Count > 0 ? arguments : null);
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

            if (Match(TokenType.Super))
            {
                ILoxToken keyword = Previous();
                Consume(TokenType.Dot, "Expected '.' after 'super'.");
                ILoxToken method = Consume(TokenType.Identifier, "Expected superclass method name.");
                return new SuperExpr(keyword, method);
            }

            if (Match(TokenType.This))
                return new ThisExpr(Previous());

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

                Advance();
            }
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

        private void AddError(string message) => AddError(Peek(), message);

        private void AddError(ILoxToken token, string message) => errors.Add(new LoxError(token, message));
    }
}
