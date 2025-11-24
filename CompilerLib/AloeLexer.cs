using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Aloe.CompilerLib.Lexer
{
    /// <summary>
    /// Aloe 言語のトークン種別
    /// </summary>
    public enum TokenKind
    {
        Unknown = 0,
        EndOfFile,

        Identifier,
        Keyword,

        IntegerLiteral,
        FloatLiteral,
        DecimalLiteral,
        StringLiteral,
        CharLiteral,
        BoolLiteral,
        NullLiteral,

        // 記号・演算子
        Plus,               // +
        Minus,              // -
        Star,               // *
        Slash,              // /
        Percent,            // %

        Equals,             // =
        EqualsEquals,       // ==
        Not,                // !
        NotEquals,          // !=
        Less,               // <
        LessOrEqual,        // <=
        Greater,            // >
        GreaterOrEqual,     // >=

        LogicalAnd,         // &&
        LogicalOr,          // ||

        Dot,                // .
        Comma,              // ,
        Colon,              // :
        Semicolon,          // ;

        LeftParen,          // (
        RightParen,         // )
        LeftBrace,          // {
        RightBrace,         // }
        LeftBracket,        // [
        RightBracket,       // ]

        Question,           // ?
    }

    /// <summary>
    /// 1 個のトークン
    /// </summary>
    public sealed class Token
    {
        public TokenKind Kind { get; }
        public string Lexeme { get; }
        public int Line { get; }
        public int Column { get; }
        public object? LiteralValue { get; }

        public Token(TokenKind kind, string lexeme, int line, int column, object? literalValue = null)
        {
            Kind = kind;
            Lexeme = lexeme;
            Line = line;
            Column = column;
            LiteralValue = literalValue;
        }

        public override string ToString()
            => $"{Kind} \"{Lexeme}\" (L{Line},C{Column})";
    }

    /// <summary>
    /// Aloe 言語の字句解析（static API）
    /// </summary>
    public static class AloeLexer
    {
        /// <summary>
        /// ソース全体を字句解析してトークン一覧を返す（AloeLexer は new しない）
        /// </summary>
        public static IReadOnlyList<Token> Parse(string? source)
        {
            var impl = new LexerImpl(source ?? string.Empty);
            return impl.Parse();
        }

        /// <summary>
        /// 内部実装用レキサ（状態を持つ）
        /// </summary>
        private sealed class LexerImpl
        {
            private readonly string _text;
            private int _position;
            private int _line;
            private int _column;

            private static readonly HashSet<string> s_keywords = new(StringComparer.Ordinal)
            {
                "abstract",
                "as",
                "break",
                "case",
                "catch",
                "class",
                "const",
                "construct",
                "continue",
                "default",
                "do",
                "else",
                "enum",
                "extends",
                "false",
                "final",
                "finally",
                "for",
                "if",
                "import",
                "in",
                "instanceof",
                "interface",
                "let",
                "method",
                "new",
                "null",
                "package",
                "private",
                "protected",
                "public",
                "return",
                "static",
                "super",
                "switch",
                "this",
                "throw",
                "throws",
                "true",
                "try",
                "var",
                "void",
                "while",
                "trait",
                "with",
                "yield",
            };

            public LexerImpl(string text)
            {
                _text = text;
                Reset();
            }

            private void Reset()
            {
                _position = 0;
                _line = 1;
                _column = 1;
            }

            public IReadOnlyList<Token> Parse()
            {
                Reset();

                var list = new List<Token>();
                Token t;
                do
                {
                    t = NextToken();
                    list.Add(t);
                } while (t.Kind != TokenKind.EndOfFile);

                return list;
            }

            private Token NextToken()
            {
                SkipWhitespaceAndComments();

                int startLine = _line;
                int startColumn = _column;
                int startPos = _position;

                char c = Current;

                if (c == '\0')
                {
                    return new Token(TokenKind.EndOfFile, string.Empty, startLine, startColumn);
                }

                // 識別子 / 予約語 / true / false / null
                if (IsIdentifierStart(c))
                {
                    return LexIdentifierOrKeyword(startLine, startColumn, startPos);
                }

                // 数値リテラル
                if (char.IsDigit(c))
                {
                    return LexNumber(startLine, startColumn, startPos);
                }

                // 文字列リテラル
                if (c == '"')
                {
                    return LexString(startLine, startColumn, startPos);
                }

                // 文字リテラル
                if (c == '\'')
                {
                    return LexChar(startLine, startColumn, startPos);
                }

                // 記号・演算子
                switch (c)
                {
                    case '+':
                        Advance();
                        return MakeSimpleToken(TokenKind.Plus, startLine, startColumn, startPos, 1);

                    case '-':
                        Advance();
                        return MakeSimpleToken(TokenKind.Minus, startLine, startColumn, startPos, 1);

                    case '*':
                        Advance();
                        return MakeSimpleToken(TokenKind.Star, startLine, startColumn, startPos, 1);

                    case '/':
                        Advance();
                        return MakeSimpleToken(TokenKind.Slash, startLine, startColumn, startPos, 1);

                    case '%':
                        Advance();
                        return MakeSimpleToken(TokenKind.Percent, startLine, startColumn, startPos, 1);

                    case '=':
                        if (Peek(1) == '=')
                        {
                            Advance(2);
                            return MakeSimpleToken(TokenKind.EqualsEquals, startLine, startColumn, startPos, 2);
                        }
                        else
                        {
                            Advance();
                            return MakeSimpleToken(TokenKind.Equals, startLine, startColumn, startPos, 1);
                        }

                    case '!':
                        if (Peek(1) == '=')
                        {
                            Advance(2);
                            return MakeSimpleToken(TokenKind.NotEquals, startLine, startColumn, startPos, 2);
                        }
                        else
                        {
                            Advance();
                            return MakeSimpleToken(TokenKind.Not, startLine, startColumn, startPos, 1);
                        }

                    case '<':
                        if (Peek(1) == '=')
                        {
                            Advance(2);
                            return MakeSimpleToken(TokenKind.LessOrEqual, startLine, startColumn, startPos, 2);
                        }
                        else
                        {
                            Advance();
                            return MakeSimpleToken(TokenKind.Less, startLine, startColumn, startPos, 1);
                        }

                    case '>':
                        if (Peek(1) == '=')
                        {
                            Advance(2);
                            return MakeSimpleToken(TokenKind.GreaterOrEqual, startLine, startColumn, startPos, 2);
                        }
                        else
                        {
                            Advance();
                            return MakeSimpleToken(TokenKind.Greater, startLine, startColumn, startPos, 1);
                        }

                    case '&':
                        if (Peek(1) == '&')
                        {
                            Advance(2);
                            return MakeSimpleToken(TokenKind.LogicalAnd, startLine, startColumn, startPos, 2);
                        }
                        break;

                    case '|':
                        if (Peek(1) == '|')
                        {
                            Advance(2);
                            return MakeSimpleToken(TokenKind.LogicalOr, startLine, startColumn, startPos, 2);
                        }
                        break;

                    case '.':
                        Advance();
                        return MakeSimpleToken(TokenKind.Dot, startLine, startColumn, startPos, 1);

                    case ',':
                        Advance();
                        return MakeSimpleToken(TokenKind.Comma, startLine, startColumn, startPos, 1);

                    case ':':
                        Advance();
                        return MakeSimpleToken(TokenKind.Colon, startLine, startColumn, startPos, 1);

                    case ';':
                        Advance();
                        return MakeSimpleToken(TokenKind.Semicolon, startLine, startColumn, startPos, 1);

                    case '(':
                        Advance();
                        return MakeSimpleToken(TokenKind.LeftParen, startLine, startColumn, startPos, 1);

                    case ')':
                        Advance();
                        return MakeSimpleToken(TokenKind.RightParen, startLine, startColumn, startPos, 1);

                    case '{':
                        Advance();
                        return MakeSimpleToken(TokenKind.LeftBrace, startLine, startColumn, startPos, 1);

                    case '}':
                        Advance();
                        return MakeSimpleToken(TokenKind.RightBrace, startLine, startColumn, startPos, 1);

                    case '[':
                        Advance();
                        return MakeSimpleToken(TokenKind.LeftBracket, startLine, startColumn, startPos, 1);

                    case ']':
                        Advance();
                        return MakeSimpleToken(TokenKind.RightBracket, startLine, startColumn, startPos, 1);

                    case '?':
                        Advance();
                        return MakeSimpleToken(TokenKind.Question, startLine, startColumn, startPos, 1);
                }

                // 未知の文字
                Advance();
                string lexUnknown = _text.Substring(startPos, _position - startPos);
                return new Token(TokenKind.Unknown, lexUnknown, startLine, startColumn);
            }

            private void SkipWhitespaceAndComments()
            {
                while (true)
                {
                    char c = Current;

                    // 空白
                    if (char.IsWhiteSpace(c))
                    {
                        Advance();
                        continue;
                    }

                    // コメント
                    if (c == '/')
                    {
                        char n = Peek(1);
                        // 行コメント: //
                        if (n == '/')
                        {
                            Advance(2);
                            while (Current != '\n' && Current != '\0')
                                Advance();
                            continue;
                        }
                        // 複数行コメント: /* ... */
                        if (n == '*')
                        {
                            Advance(2);
                            while (true)
                            {
                                if (Current == '\0')
                                    return; // EOF

                                if (Current == '*' && Peek(1) == '/')
                                {
                                    Advance(2);
                                    break;
                                }

                                Advance();
                            }
                            continue;
                        }
                    }

                    break;
                }
            }

            private Token LexIdentifierOrKeyword(int startLine, int startColumn, int startPos)
            {
                var sb = new StringBuilder();
                while (IsIdentifierPart(Current))
                {
                    sb.Append(Current);
                    Advance();
                }

                string lexeme = sb.ToString();

                if (s_keywords.Contains(lexeme))
                {
                    if (lexeme == "true")
                        return new Token(TokenKind.BoolLiteral, lexeme, startLine, startColumn, true);
                    if (lexeme == "false")
                        return new Token(TokenKind.BoolLiteral, lexeme, startLine, startColumn, false);
                    if (lexeme == "null")
                        return new Token(TokenKind.NullLiteral, lexeme, startLine, startColumn, null);

                    return new Token(TokenKind.Keyword, lexeme, startLine, startColumn);
                }

                return new Token(TokenKind.Identifier, lexeme, startLine, startColumn);
            }

            private Token LexNumber(int startLine, int startColumn, int startPos)
            {
                bool isHex = false;
                bool isBin = false;
                bool isFloat = false;

                if (Current == '0' && (Peek(1) == 'x' || Peek(1) == 'X'))
                {
                    isHex = true;
                    Advance(2); // "0x"
                    while (IsHexDigit(Current))
                        Advance();
                }
                else if (Current == '0' && (Peek(1) == 'b' || Peek(1) == 'B'))
                {
                    isBin = true;
                    Advance(2); // "0b"
                    while (Current == '0' || Current == '1')
                        Advance();
                }
                else
                {
                    while (char.IsDigit(Current))
                        Advance();

                    if (Current == '.' && char.IsDigit(Peek(1)))
                    {
                        isFloat = true;
                        Advance(); // '.'
                        while (char.IsDigit(Current))
                            Advance();
                    }
                }

                int valueEndPos = _position;

                char suffix = '\0';
                if (Current == ':' && IsLiteralSuffix(Peek(1)))
                {
                    suffix = Peek(1);
                    Advance(2); // ':' + suffix
                }

                int endPos = _position;
                string lexeme = _text.Substring(startPos, endPos - startPos);

                TokenKind kind;
                object? literalValue = null;

                if (suffix == 'd' || suffix == 'D')
                {
                    kind = TokenKind.DecimalLiteral;
                    if (!isHex && !isBin)
                    {
                        string numericText = _text.Substring(startPos, valueEndPos - startPos);
                        if (decimal.TryParse(numericText, NumberStyles.Float, CultureInfo.InvariantCulture, out var dec))
                            literalValue = dec;
                    }
                }
                else if (isFloat || suffix == 'f' || suffix == 'F')
                {
                    kind = TokenKind.FloatLiteral;
                    if (!isHex && !isBin)
                    {
                        string numericText = _text.Substring(startPos, valueEndPos - startPos);
                        if (double.TryParse(numericText, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
                            literalValue = d;
                    }
                }
                else
                {
                    kind = TokenKind.IntegerLiteral;
                    string numericText = _text.Substring(startPos, valueEndPos - startPos);

                    if (isHex)
                        literalValue = ParseHexInteger(numericText);
                    else if (isBin)
                        literalValue = ParseBinaryInteger(numericText);
                    else if (long.TryParse(numericText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l))
                        literalValue = l;
                }

                return new Token(kind, lexeme, startLine, startColumn, literalValue);
            }

            private Token LexString(int startLine, int startColumn, int startPos)
            {
                Advance(); // 開き "

                var sb = new StringBuilder();

                while (true)
                {
                    char c = Current;
                    if (c == '\0')
                        break;

                    if (c == '"')
                    {
                        Advance();
                        break;
                    }

                    if (c == '\\')
                    {
                        char n = Peek(1);
                        Advance(2);

                        sb.Append(n switch
                        {
                            'n' => '\n',
                            'r' => '\r',
                            't' => '\t',
                            '\\' => '\\',
                            '"' => '"',
                            '\'' => '\'',
                            '0' => '\0',
                            _ => n,
                        });
                    }
                    else
                    {
                        sb.Append(c);
                        Advance();
                    }
                }

                int endPos = _position;
                string lexeme = _text.Substring(startPos, endPos - startPos);
                string value = sb.ToString();

                return new Token(TokenKind.StringLiteral, lexeme, startLine, startColumn, value);
            }

            private Token LexChar(int startLine, int startColumn, int startPos)
            {
                Advance(); // 開き '

                char value = '\0';
                bool hasValue = false;

                if (Current == '\\')
                {
                    char n = Peek(1);
                    Advance(2);
                    value = n switch
                    {
                        'n' => '\n',
                        'r' => '\r',
                        't' => '\t',
                        '\\' => '\\',
                        '"' => '"',
                        '\'' => '\'',
                        '0' => '\0',
                        _ => n,
                    };
                    hasValue = true;
                }
                else if (Current != '\0' && Current != '\'' && Current != '\n')
                {
                    value = Current;
                    Advance();
                    hasValue = true;
                }

                if (Current == '\'')
                    Advance();

                int endPos = _position;
                string lexeme = _text.Substring(startPos, endPos - startPos);

                if (!hasValue)
                    return new Token(TokenKind.Unknown, lexeme, startLine, startColumn);

                return new Token(TokenKind.CharLiteral, lexeme, startLine, startColumn, value);
            }

            // ===== ヘルパ =====

            private bool IsIdentifierStart(char c)
                => c == '_' || char.IsLetter(c);

            private bool IsIdentifierPart(char c)
                => c == '_' || char.IsLetterOrDigit(c);

            private static bool IsHexDigit(char c)
                => char.IsDigit(c)
                   || (c >= 'a' && c <= 'f')
                   || (c >= 'A' && c <= 'F');

            private static bool IsLiteralSuffix(char c)
                => c is 'i' or 'I' or 'f' or 'F' or 'd' or 'D';

            private long? ParseHexInteger(string text)
            {
                if (!text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                    return null;

                long value = 0;
                for (int i = 2; i < text.Length; i++)
                {
                    char c = text[i];
                    int digit;
                    if (c >= '0' && c <= '9')
                        digit = c - '0';
                    else if (c >= 'a' && c <= 'f')
                        digit = 10 + (c - 'a');
                    else if (c >= 'A' && c <= 'F')
                        digit = 10 + (c - 'A');
                    else
                        return null;

                    value = (value << 4) + digit;
                }

                return value;
            }

            private long? ParseBinaryInteger(string text)
            {
                if (!text.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
                    return null;

                long value = 0;
                for (int i = 2; i < text.Length; i++)
                {
                    char c = text[i];
                    if (c != '0' && c != '1')
                        return null;

                    value = (value << 1) + (c == '1' ? 1 : 0);
                }

                return value;
            }

            private Token MakeSimpleToken(TokenKind kind, int line, int column, int startPos, int length)
            {
                string lexeme = _text.Substring(startPos, length);
                return new Token(kind, lexeme, line, column);
            }

            private char Current => _position < _text.Length ? _text[_position] : '\0';

            private char Peek(int offset)
            {
                int index = _position + offset;
                if (index < 0 || index >= _text.Length)
                    return '\0';
                return _text[index];
            }

            private void Advance(int count = 1)
            {
                for (int i = 0; i < count; i++)
                {
                    if (_position >= _text.Length)
                        return;

                    char c = _text[_position++];
                    if (c == '\n')
                    {
                        _line++;
                        _column = 1;
                    }
                    else
                    {
                        _column++;
                    }
                }
            }
        }
    }
}
