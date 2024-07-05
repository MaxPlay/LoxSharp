﻿

namespace LoxSharp.Core
{
    public class Interpreter : IVisitor<RuntimeValue>
    {
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
                    return null;
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
                _ => null
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

        public string Interpret(IExpr expression, out LoxError? error)
        {
            error = null;
            try
            {
                return Evaluate(expression).ToString();
            }
            catch (LoxRuntimeException ex)
            {
                error = ex.Error;
                return string.Empty;
            }
        }
    }
}