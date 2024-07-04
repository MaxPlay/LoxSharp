namespace LoxSharp.Core
{
    public class LoxError(int line, string where, string message)
    {
        public int Line { get; } = line;
        public string Where { get; } = where;
        public string Message { get; } = message;
    }
}