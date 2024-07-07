namespace LoxSharp.Core
{
    public class LoxError
    {
        public LoxError(int line, string where, string message)
        {
            Line = line;
            Where = where;
            Message = message;
        }

        public LoxError(ILoxToken token, string message)
        {
            Line = token.Line;
            Where = token.Type == TokenType.Eof
                ? " at end"
                : " at '" + token.Lexeme + "'";
            Message = message;
        }

        public int Line { get; }
        public string Where { get; }
        public string Message { get; }
    }
}