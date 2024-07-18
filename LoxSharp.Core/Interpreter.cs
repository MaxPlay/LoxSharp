﻿namespace LoxSharp.Core
{
    public class Interpreter : IExprVisitor<RuntimeValue>, IStmtVisitor<object?>
    {
        private readonly TextWriter outWriter;
        private readonly TextWriter outError;

        public LoxEnvironment Globals => globals;
        public LoxEnvironment Environment => environment;
        private readonly LoxEnvironment globals = new LoxEnvironment();
        private LoxEnvironment environment;

        private readonly Dictionary<IExpr, int> locals = [];

        public Interpreter(TextWriter outWriter, TextWriter outError)
        {
            this.outWriter = outWriter;
            this.outError = outError;
            environment = globals;

            globals.Define(NativeLoxCallable.Make("clock", () => Math.Floor((DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds)));
            globals.Define(NativeLoxCallable.Make("platypus", (count) => $"{count.NumericValue} little platypus!"));
        }

        public void Interpret(List<IStmt> statements)
        {
            try
            {
                foreach (var stmt in statements)
                {
                    Execute(stmt);
                }
            }
            catch (LoxRuntimeException ex)
            {
                if (ex.Error != null)
                    outError.WriteLine($"Error: [{ex.Error.Line}] Error{ex.Error.Where}: {ex.Error.Message}");
                else
                    outError.WriteLine(ex);
            }
        }

        private void Execute(IStmt stmt) => stmt.Accept(this);

        public void ExecuteBlock(List<IStmt> statements, LoxEnvironment environment)
        {
            LoxEnvironment outerEnvironment = this.environment;
            try
            {
                this.environment = environment;

                foreach (IStmt statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                this.environment = outerEnvironment;
            }
        }

        // - IExprVisitor -

        public RuntimeValue Visit(AssignExpr expr)
        {
            RuntimeValue value = Evaluate(expr.Value);
            if (locals.TryGetValue(expr, out int depth))
                environment.AssignAt(depth, expr.Name, value);
            else
                environment.Assign(expr.Name, ref value);
            return value;
        }

        public RuntimeValue Visit(BinaryExpr expr)
        {
            RuntimeValue left = Evaluate(expr.Left);
            RuntimeValue right = Evaluate(expr.Right);

            switch (expr.Op.Type)
            {
                case TokenType.Minus:
                    CheckNumberOperand(expr.Op, ref left, ref right);
                    return left.NumericValue - right.NumericValue;
                case TokenType.Slash:
                    CheckNumberOperand(expr.Op, ref left, ref right);
                    return left.NumericValue / right.NumericValue;
                case TokenType.Star:
                    CheckNumberOperand(expr.Op, ref left, ref right);
                    return left.NumericValue * right.NumericValue;
                case TokenType.Plus:
                    {
                        if (left.Type == RuntimeValueType.String || right.Type == RuntimeValueType.String)
                            return left.StringValue + right.StringValue;

                        CheckNumberOperand(expr.Op, ref left, ref right);
                        return left.NumericValue + right.NumericValue;
                    }
                case TokenType.Greater:
                    CheckNumberOperand(expr.Op, ref left, ref right);
                    return left.NumericValue > right.NumericValue;
                case TokenType.GreaterEqual:
                    CheckNumberOperand(expr.Op, ref left, ref right);
                    return left.NumericValue >= right.NumericValue;
                case TokenType.Less:
                    CheckNumberOperand(expr.Op, ref left, ref right);
                    return left.NumericValue < right.NumericValue;
                case TokenType.LessEqual:
                    CheckNumberOperand(expr.Op, ref left, ref right);
                    return left.NumericValue <= right.NumericValue;
                case TokenType.BangEqual:
                    return !IsEqual(left, right);
                case TokenType.EqualEqual:
                    return IsEqual(left, right);
                default:
                    return RuntimeValue.NullValue;
            }
        }

        public RuntimeValue Visit(GroupingExpr expr)
        {
            return Evaluate(expr.Expression);
        }

        public RuntimeValue Visit(LiteralExpr expr)
        {
            return expr.Value switch
            {
                LiteralBoolValue boolValue => boolValue.Value,
                LiteralStringValue stringValue => stringValue.Value,
                LiteralNumericValue numericValue => numericValue.Value,
                _ => RuntimeValue.NullValue
            };
        }

        public RuntimeValue Visit(UnaryExpr expr)
        {
            RuntimeValue right = Evaluate(expr.Right);

            switch (expr.Op.Type)
            {
                case TokenType.Minus:
                    CheckNumberOperand(expr.Op, ref right);
                    return -right.NumericValue;
                case TokenType.Bang:
                    return !right.BoolValue;
                default:
                    throw new NotImplementedException();
            };
        }

        public RuntimeValue Visit(VariableExpr expr)
        {
            LookupVariable(expr.Name, expr, out RuntimeValue value);
            return value;
        }

        public RuntimeValue Visit(LogicalExpr expr)
        {
            RuntimeValue left = Evaluate(expr.Left);

            if (expr.Op.Type == TokenType.Or)
            {
                if (left.BoolValue)
                    return left;
            }
            else
            {
                if (!left.BoolValue)
                    return left;
            }

            return Evaluate(expr.Right);
        }

        public RuntimeValue Visit(CallExpr expr)
        {
            RuntimeValue callee = Evaluate(expr.Callee);

            List<RuntimeValue>? arguments = null;
            if (expr.Arguments != null)
            {
                arguments = [];
                foreach (IExpr arg in expr.Arguments)
                {
                    arguments.Add(Evaluate(arg));
                }
            }

            if ((callee.Type != RuntimeValueType.Function || callee.FunctionValue == null) && (callee.Type != RuntimeValueType.Class || callee.ClassValue == null))
                throw new LoxRuntimeException(expr.Parent, "Can only call functions and classes.");
            ILoxCallable? function = callee.Type == RuntimeValueType.Function ? callee.FunctionValue : callee.ClassValue;
            int argumentCount = arguments?.Count ?? 0;
            if (argumentCount != function?.Arity)
                throw new LoxRuntimeException(expr.Parent, $"Expected {function?.Arity} arguments but got {argumentCount}.");

            return function.Call(this, arguments);
        }

        // - IStmtVisitor -

        public object? Visit(ExpressionStmt stmt)
        {
            Evaluate(stmt.Expression);
            return null;
        }

        public object? Visit(PrintStmt stmt)
        {
            RuntimeValue runtimeValue = Evaluate(stmt.Expression);
            outWriter.WriteLine(runtimeValue.StringValue);
            return null;
        }

        public object? Visit(VarStmt stmt)
        {
            RuntimeValue value = Evaluate(stmt.Initializer);

            environment.Define(stmt.Name.Lexeme, ref value);
            return null;
        }

        public object? Visit(BlockStmt stmt)
        {
            ExecuteBlock(stmt.Statements, new LoxEnvironment(environment));

            return null;
        }

        public object? Visit(IfStmt stmt)
        {
            if (Evaluate(stmt.Condition).BoolValue)
                Execute(stmt.ThenBranch);
            else if (stmt.ElseBranch != null)
                Execute(stmt.ElseBranch);

            return null;
        }

        public object? Visit(WhileStmt stmt)
        {
            while (Evaluate(stmt.Condition).BoolValue)
            {
                Execute(stmt.Body);
            }
            return null;
        }

        public object? Visit(FunctionStmt stmt)
        {
            LoxFunction function = new LoxFunction(stmt, environment);
            environment.Define(function);
            return null;
        }

        public object? Visit(ReturnStmt stmt)
        {
            RuntimeValue? value = null;
            if (stmt.Value != null)
                value = Evaluate(stmt.Value);

            throw new ReturnException(value);
        }

        public object? Visit(ClassStmt stmt)
        {
            environment.Define(stmt.Name.Lexeme);
            LoxClass loxClass = new LoxClass(stmt.Name.Lexeme);
            environment.Assign(stmt.Name, loxClass);
            return null;
        }

        // - Helpers -

        private RuntimeValue Evaluate(IExpr expression) => expression.Accept(this);

        private static bool IsEqual(RuntimeValue left, RuntimeValue right)
        {
            if (left.IsNil)
                return right.IsNil;

            if (left.Type != right.Type)
                return false;

            return left.Type switch
            {
                RuntimeValueType.Boolean => left.BoolValue == right.BoolValue,
                RuntimeValueType.Numeric => left.NumericValue == right.NumericValue,
                RuntimeValueType.String => left.StringValue == right.StringValue,
                _ => false,
            };
        }

        private static void CheckNumberOperand(ILoxToken op, ref RuntimeValue operand)
        {
            if (operand.Type != RuntimeValueType.Numeric)
                throw new LoxRuntimeException(op, "Operand must be a number.");
        }

        private static void CheckNumberOperand(ILoxToken op, ref RuntimeValue right, ref RuntimeValue left)
        {
            if (right.Type != RuntimeValueType.Numeric || left.Type != RuntimeValueType.Numeric)
                throw new LoxRuntimeException(op, "Operands must be a numbers.");
        }

        public void Resolve(IExpr expression, int depth)
        {
            locals[expression] = depth;
        }

        private void LookupVariable(ILoxToken name, VariableExpr expr, out RuntimeValue value)
        {
            if (locals.TryGetValue(expr, out int depth))
                environment.GetAt(depth, name.Lexeme, out value);
            else
                globals.Get(name, out value);
        }
    }
}
