namespace LoxSharp.Core
{
    public class ScanError(int line, string message)
    {
        public int Line { get; } = line;
        public string Message { get; } = message;
    }
}