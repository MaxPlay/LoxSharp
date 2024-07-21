namespace LoxSharp.Core
{
    public class Resolver(Interpreter interpreter) : IExprVisitor<object?>, IStmtVisitor<object?>
    {
        private enum FunctionType
        {
            None,
            Function,
            Initializer,
            Method
        }

        private enum ClassType
        {
            None,
            Class,
            Subclass
        }

        private readonly Interpreter interpreter = interpreter;
        private readonly List<Dictionary<string, bool>> scopes = [];
        private readonly List<LoxError> errors = [];
        public IReadOnlyList<LoxError> Errors => errors;
        private FunctionType currentFunction = FunctionType.None;
        private ClassType currentClass = ClassType.None;

        // IStmtVisitor implementation

        public object? Visit(BlockStmt stmt)
        {
            BeginScope();
            Resolve(stmt.Statements);
            EndScope();
            return null;
        }

        public object? Visit(ClassStmt stmt)
        {
            ClassType enclosingClass = currentClass;
            currentClass = ClassType.Class;

            Declare(stmt.Name);
            Define(stmt.Name);

            if (stmt.Superclass != null)
            {
                if (stmt.Name.Lexeme != stmt.Superclass.Name.Lexeme)
                {
                    currentClass = ClassType.Subclass;
                    Resolve(stmt.Superclass);
                    BeginScope();
                    scopes[^1]["super"] = true;
                }
                else
                {
                    AddError(stmt.Superclass.Name, "A class can't interhit from itself.");
                }
            }

            BeginScope();
            scopes[^1]["this"] = true;

            foreach (FunctionStmt method in stmt.Methods)
            {
                FunctionType type = method.Name.Lexeme == LoxFunction.INITIALIZER_KEYWORD ? FunctionType.Initializer : FunctionType.Method;
                ResolveFunction(method, type);
            }
            EndScope();
            if (stmt.Superclass != null)
                EndScope();

            currentClass = enclosingClass;
            return null;
        }

        public object? Visit(ExpressionStmt stmt)
        {
            Resolve(stmt.Expression);
            return null;
        }

        public object? Visit(FunctionStmt stmt)
        {
            Declare(stmt.Name);
            Define(stmt.Name);

            ResolveFunction(stmt, FunctionType.Function);
            return null;
        }

        public object? Visit(IfStmt stmt)
        {
            Resolve(stmt.Condition);
            Resolve(stmt.ThenBranch);
            if (stmt.ElseBranch != null)
                Resolve(stmt.ElseBranch);
            return null;
        }

        public object? Visit(PrintStmt stmt)
        {
            Resolve(stmt.Expression);
            return null;
        }

        public object? Visit(ReturnStmt stmt)
        {
            if (currentFunction == FunctionType.None)
                AddError(stmt.Keyword, "Can't return from top-level code.");

            if (stmt.Value != null)
            {
                if (currentFunction == FunctionType.Initializer)
                    AddError(stmt.Keyword, "Can't return a value from an initializer.");
                Resolve(stmt.Value);
            }
            return null;
        }

        public object? Visit(VarStmt stmt)
        {
            Declare(stmt.Name);
            if (stmt.Initializer != null)
                Resolve(stmt.Initializer);
            Define(stmt.Name);
            return null;
        }

        public object? Visit(WhileStmt stmt)
        {
            Resolve(stmt.Condition);
            Resolve(stmt.Body);
            return null;
        }

        // IExprVisitor implementation

        public object? Visit(AssignExpr expr)
        {
            Resolve(expr.Value);
            ResolveLocal(expr, expr.Name);
            return null;
        }

        public object? Visit(BinaryExpr expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);
            return null;
        }

        public object? Visit(CallExpr expr)
        {
            Resolve(expr.Callee);

            if (expr.Arguments != null)
            {
                foreach (IExpr arg in expr.Arguments)
                {
                    Resolve(arg);
                }
            }
            return null;
        }

        public object? Visit(GetExpr expr)
        {
            Resolve(expr.Obj);
            return null;
        }

        public object? Visit(GroupingExpr expr)
        {
            Resolve(expr);
            return null;
        }

        public object? Visit(LiteralExpr expr) => null;

        public object? Visit(LogicalExpr expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);
            return null;
        }

        public object? Visit(SetExpr expr)
        {
            Resolve(expr.Value);
            Resolve(expr.Obj);
            return null;
        }

        public object? Visit(SuperExpr expr)
        {
            switch (currentClass)
            {
                case ClassType.None:
                    AddError(expr.Keyword, "Can't use 'super' keyword outside of a class.");
                    break;
                case ClassType.Class:
                    AddError(expr.Keyword, "Can't use 'super' keyword in a class with no superclass.");
                    break;
            }

            ResolveLocal(expr, expr.Keyword);
            return null;
        }

        public object? Visit(ThisExpr expr)
        {
            if (currentClass == ClassType.None)
                AddError(expr.Keyword, "Can't use 'this' keyword outside of a class.");

            ResolveLocal(expr, expr.Keyword);
            return null;
        }

        public object? Visit(UnaryExpr expr)
        {
            Resolve(expr.Right);
            return null;
        }

        public object? Visit(VariableExpr expr)
        {
            if (scopes.Count != 0 && scopes[^1].TryGetValue(expr.Name.Lexeme, out bool isDefined) && !isDefined)
                AddError(expr.Name, "Can't read local variable in its own initializer.");
            ResolveLocal(expr, expr.Name);
            return null;
        }

        // Helpers

        private void BeginScope() => scopes.Add([]);

        private void EndScope() => scopes.RemoveAt(scopes.Count - 1);

        private void Resolve(IExpr expression) => expression.Accept(this);

        private void Resolve(IStmt statement) => statement.Accept(this);

        public void Resolve(List<IStmt> statements)
        {
            foreach (IStmt statement in statements)
            {
                Resolve(statement);
            }
        }

        private void ResolveLocal(IExpr expression, ILoxToken token)
        {
            for (int i = scopes.Count - 1; i >= 0; i--)
            {
                if (scopes[i].ContainsKey(token.Lexeme))
                {
                    interpreter.Resolve(expression, scopes.Count - 1 - i);
                    return;
                }
            }
        }

        private void Declare(ILoxToken name)
        {
            if (scopes.Count == 0)
                return;

            Dictionary<string, bool> scope = scopes[^1];
            if (scope.ContainsKey(name.Lexeme))
                AddError(name, "Already a variable defined with this name in the scope.");
            scope.Add(name.Lexeme, false);
        }

        private void Define(ILoxToken name)
        {
            if (scopes.Count == 0)
                return;
            scopes[^1][name.Lexeme] = true;
        }

        private void ResolveFunction(FunctionStmt function, FunctionType type)
        {
            FunctionType enclosingFunction = currentFunction;
            currentFunction = type;
            BeginScope();
            foreach (ILoxToken token in function.Parameters)
            {
                Declare(token);
                Define(token);
            }
            Resolve(function.Body);
            EndScope();
            currentFunction = enclosingFunction;
        }

        private void AddError(ILoxToken token, string message) => errors.Add(new LoxError(token, message));
    }
}
