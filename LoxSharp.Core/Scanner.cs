
using System.ComponentModel.DataAnnotations;

namespace LoxSharp.Core
{
    public enum TokenType
    {
        // Single-character tokens
        LeftParenthesis,
        RightParenthesis,
        LeftBrace,
        RightBrace,
        Comma,
        Dot,
        Minus,
        Plus,
        Semicolon,
        Slash,
        Star,
        // One or two character tokens
        Bang,
        BangEqual,
        Equal,
        EqualEqual,
        Greater,
        GreaterEqual,
        Less,
        LessEqual,
        // Literals
        Identifier,
        String,
        Number,
        // Keywords
        And,
        Class,
        Else,
        False,
        Function,
        For,
        If,
        Nil,
        Or,
        Print,
        Return,
        Super,
        This,
        True,
        Var,
        While,
        // Control symbols
        Eof
    }

    public interface ILoxToken
    {
    }

    public class LoxToken(TokenType type, string lexeme, int line) : ILoxToken
    {
        private readonly TokenType type = type;
        private readonly string lexeme = lexeme;
        private readonly int line = line;

        public override string ToString() => $"{type} {lexeme}";
    }

    public class LoxTokenNumeric(TokenType type, string lexeme, int line, double literal) : ILoxToken
    {
        private readonly TokenType type = type;
        private readonly string lexeme = lexeme;
        private readonly int line = line;
        private readonly double literal = literal;

        public override string ToString() => $"{type} {lexeme} {literal}";
    }

    public class LoxTokenString(TokenType type, string lexeme, int line, string literal) : ILoxToken
    {
        private readonly TokenType type = type;
        private readonly string lexeme = lexeme;
        private readonly int line = line;
        private readonly string literal = literal;

        public override string ToString() => $"{type} {lexeme} {literal}";
    }

    public class Scanner
    {
        private readonly string source;
        private readonly List<ILoxToken> tokens = [];

        private readonly Dictionary<string, TokenType> keywordMapping;

        private int start;
        private int current;
        private int line;

        public Scanner(string[] source)
        {
            this.source = string.Join('\n', source);
            keywordMapping = new Dictionary<string, TokenType>()
            {
                ["and"] = TokenType.And,
                ["class"] = TokenType.Class,
                ["else"] = TokenType.Else,
                ["false"] = TokenType.False,
                ["for"] = TokenType.For,
                ["fun"] = TokenType.Function,
                ["if"] = TokenType.If,
                ["nil"] = TokenType.Nil,
                ["or"] = TokenType.Or,
                ["print"] = TokenType.Print,
                ["return"] = TokenType.Return,
                ["super"] = TokenType.Super,
                ["this"] = TokenType.This,
                ["true"] = TokenType.True,
                ["var"] = TokenType.Var,
                ["while"] = TokenType.While
            };
        }

        public IReadOnlyList<ILoxToken> Tokenize(out List<ScanError> errors)
        {
            errors = [];
            line = 0;
            tokens.Clear();

            while (!IsAtEnd())
            {
                start = current;
                ScanToken(errors);
            }

            tokens.Add(new LoxToken(TokenType.Eof, string.Empty, line));

            return tokens;
        }

        private void ScanToken(List<ScanError> errors)
        {
            char c = Advance();
            switch (c)
            {
                case '(': AddToken(TokenType.LeftParenthesis); break;
                case ')': AddToken(TokenType.RightParenthesis); break;
                case '{': AddToken(TokenType.LeftBrace); break;
                case '}': AddToken(TokenType.RightBrace); break;
                case ',': AddToken(TokenType.Comma); break;
                case '.': AddToken(TokenType.Dot); break;
                case '-': AddToken(TokenType.Minus); break;
                case '+': AddToken(TokenType.Plus); break;
                case ';': AddToken(TokenType.Semicolon); break;
                case '*': AddToken(TokenType.Star); break;
                case '!':
                    AddToken(Match('=') ? TokenType.BangEqual : TokenType.Bang);
                    break;
                case '=':
                    AddToken(Match('=') ? TokenType.EqualEqual : TokenType.Equal);
                    break;
                case '<':
                    AddToken(Match('=') ? TokenType.LessEqual : TokenType.Less);
                    break;
                case '>':
                    AddToken(Match('=') ? TokenType.GreaterEqual : TokenType.Greater);
                    break;
                case '/':
                    if (Match('/'))
                    {
                        // A comment goes until the end of the line
                        while (Peek() != '\n' && !IsAtEnd())
                            Advance();
                    }
                    else
                    {
                        AddToken(TokenType.Slash);
                    }
                    break;
                case ' ':
                case '\t':
                case '\r':
                    // Ignore whitespace
                    break;
                case '\n':
                    line++;
                    break;

                case '"':
                    HandleString(errors);
                    break;

                default:
                    if (char.IsDigit(c))
                        HandleNumber();
                    else if (char.IsLetter(c))
                        HandleIdentifier();
                    else
                        errors.Add(new ScanError(line, "Unexpected character."));
                    break;
            }
        }

        private void HandleIdentifier()
        {
            while (char.IsLetterOrDigit(Peek()))
                Advance();

            string text = source[start..current];
            if (!keywordMapping.TryGetValue(text, out var keyword))
            {
                keyword = TokenType.Identifier;
            }

            AddToken(keyword);
        }

        private void HandleNumber()
        {
            while (char.IsDigit(Peek()))
                Advance();

            // Look for the fractional part.
            if (Peek() == '.' && char.IsDigit(PeekNext()))
            {
                // Consume the '.'
                Advance();

                while (char.IsDigit(Peek()))
                    Advance();
            }

            AddToken(double.Parse(source[start..current])); // Should be fine, since we checked the characters
        }

        private void HandleString(List<ScanError> errors)
        {
            while (Peek() != '"' && !IsAtEnd())
            {
                if (Peek() == '\n')
                    line++;
                Advance();
            }

            if (IsAtEnd())
            {
                errors.Add(new ScanError(line, "Unterminated string."));
                return;
            }

            // The closing '"'
            Advance();

            // Trim the surrounding quotes
            string value = source.Substring(start + 1, current - start - 2);
            AddToken(value);
        }

        private char Peek() => !IsAtEnd() ? source[current] : '\0';
        private char PeekNext() => current + 1 >= source.Length ? '\0' : source[current + 1];

        private bool Match(char expected)
        {
            if (IsAtEnd() || source[current] != expected)
                return false;

            current++;
            return true;
        }

        private char Advance() => source[current++];

        private void AddToken(TokenType type) => tokens.Add(new LoxToken(type, source[start..current], line));

        private void AddToken(string literal) => tokens.Add(new LoxTokenString(TokenType.String, source[start..current], line, literal));
        private void AddToken(double literal) => tokens.Add(new LoxTokenNumeric(TokenType.Number, source[start..current], line, literal));

        private bool IsAtEnd() => current >= source.Length;
    }
}
