
namespace LoxSharp.Core
{
    [Serializable]
    internal class ReturnException : Exception
    {
        public RuntimeValue? Value { get; }

        protected ReturnException()
        {
        }

        public ReturnException(RuntimeValue? value)
        {
            Value = value;
        }

        protected ReturnException(string? message) : base(message)
        {
        }

        protected ReturnException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}