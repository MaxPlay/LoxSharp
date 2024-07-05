

namespace LoxSharp.Core
{
    [Serializable]
    public class LoxRuntimeException : Exception
    {
        public ILoxToken? Operator { get; }
        public LoxError? Error { get; }

        protected LoxRuntimeException()
        {
        }

        protected LoxRuntimeException(string? message) : base(message)
        {
        }

        public LoxRuntimeException(ILoxToken op, string message) : base(message)
        {
            Operator = op;
            Error = new LoxError(op.Line, string.Empty, message);
        }

        protected LoxRuntimeException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
