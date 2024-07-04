namespace LoxSharp.Core
{
    [Serializable]
    public class LoxParseException : Exception
    {
        public ILoxToken? Token { get; }
        public LoxError? Error { get; }

        protected LoxParseException()
        {
        }

        protected LoxParseException(string? message) : base(message)
        {
        }

        public LoxParseException(ILoxToken token, string message) : base(message)
        {
            Token = token;

            Error = token.Type == TokenType.Eof
                ? new LoxError(token.Line, " at end", message)
                : new LoxError(token.Line, " at '" + token.Lexeme + "'", message);
        }

        protected LoxParseException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
