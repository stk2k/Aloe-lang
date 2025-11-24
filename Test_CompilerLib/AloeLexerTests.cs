using Aloe.CompilerLib.Lexer;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Linq;

namespace Aloe.CompilerLib.Tests
{
    [TestFixture]
    public class AloeLexerTests
    {
        /// <summary>
        /// tokens 内に (kind, lexeme) の並びが連続して現れることを検証するヘルパー
        /// </summary>
        private static void AssertContainsSequence(
            Token[] tokens,
            (TokenKind kind, string lexeme)[] pattern,
            string? message = null)
        {
            for (int i = 0; i <= tokens.Length - pattern.Length; i++)
            {
                bool ok = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (tokens[i + j].Kind != pattern[j].kind ||
                        tokens[i + j].Lexeme != pattern[j].lexeme)
                    {
                        ok = false;
                        break;
                    }
                }

                if (ok)
                    return; // 見つかった
            }

            Assert.Fail(message ?? $"Token sequence not found: {string.Join(" ", pattern.Select(p => $"{p.kind} \"{p.lexeme}\""))}");
        }

        private static void AssertToken(
            Token token,
            TokenKind kind,
            string lexeme)
        {
            Assert.That(token.Kind, Is.EqualTo(kind), $"Kind mismatch for lexeme '{token.Lexeme}'");
            Assert.That(token.Lexeme, Is.EqualTo(lexeme), $"Lexeme mismatch for kind '{kind}'");
        }

        // 必要なら行・列もチェックする版
        private static void AssertToken(
            Token token,
            TokenKind kind,
            string lexeme,
            int line,
            int column)
        {
            Assert.That(token.Kind, Is.EqualTo(kind), "Kind mismatch");
            Assert.That(token.Lexeme, Is.EqualTo(lexeme), "Lexeme mismatch");
            Assert.That(token.Line, Is.EqualTo(line), "Line mismatch");
            Assert.That(token.Column, Is.EqualTo(column), "Column mismatch");
        }

        [Test]
        public void KeywordAndIdentifier_AreTokenizedCorrectly()
        {
            // class FizzBuzz {}
            var source = "class FizzBuzz {}";

            var tokens = AloeLexer.Parse(source).ToArray();

            Assert.That(tokens.Length, Is.EqualTo(5)); // class, FizzBuzz, {, }, EOF

            AssertToken(tokens[0], TokenKind.Keyword, "class");
            AssertToken(tokens[1], TokenKind.Identifier, "FizzBuzz");
            AssertToken(tokens[2], TokenKind.LeftBrace, "{");
            AssertToken(tokens[3], TokenKind.RightBrace, "}");
            AssertToken(tokens[4], TokenKind.EndOfFile, "");
        }

        [Test]
        public void BoolAndNullKeywords_BecomeLiteralTokens()
        {
            var source = "true false null";
            var tokens = AloeLexer.Parse(source).Where(t => t.Kind != TokenKind.EndOfFile).ToArray();

            Assert.That(tokens.Length, Is.EqualTo(3));

            AssertToken(tokens[0], TokenKind.BoolLiteral, "true");
            Assert.That(tokens[0].LiteralValue, Is.EqualTo(true));

            AssertToken(tokens[1], TokenKind.BoolLiteral, "false");
            Assert.That(tokens[1].LiteralValue, Is.EqualTo(false));

            AssertToken(tokens[2], TokenKind.NullLiteral, "null");
            Assert.That(tokens[2].LiteralValue, Is.Null);
        }

        [Test]
        public void IntegerFloatDecimalAndRadixLiterals_AreParsed()
        {
            var source = @"
                var a = 10;
                var b = 10.5;
                var c = 10.25:d;
                var d = 0b1010;
                var e = 0x1F;
            ";

            var tokens = AloeLexer.Parse(source)
                .Where(t => t.Kind == TokenKind.IntegerLiteral
                         || t.Kind == TokenKind.FloatLiteral
                         || t.Kind == TokenKind.DecimalLiteral)
                .ToArray();

            // 10, 10.5, 10.25:d, 0b1010, 0x1F の 5 つ
            Assert.That(tokens.Length, Is.EqualTo(5));

            AssertToken(tokens[0], TokenKind.IntegerLiteral, "10");
            Assert.That(tokens[0].LiteralValue, Is.EqualTo(10L));

            AssertToken(tokens[1], TokenKind.FloatLiteral, "10.5");
            Assert.That(tokens[1].LiteralValue, Is.EqualTo(10.5d));

            AssertToken(tokens[2], TokenKind.DecimalLiteral, "10.25:d");
            Assert.That(tokens[2].LiteralValue, Is.TypeOf<decimal>());

            AssertToken(tokens[3], TokenKind.IntegerLiteral, "0b1010");
            Assert.That(tokens[3].LiteralValue, Is.EqualTo(10L));  // 2進 1010 → 10

            AssertToken(tokens[4], TokenKind.IntegerLiteral, "0x1F");
            Assert.That(tokens[4].LiteralValue, Is.EqualTo(31L));  // 16進 1F → 31
        }

        [Test]
        public void StringAndCharLiterals_AreParsedWithEscapes()
        {
            var source = @"var s = ""Hi\n""; var c = '\n';";
            var tokens = AloeLexer.Parse(source).ToArray();

            // string / char だけ抜き出して検証
            var stringToken = tokens.First(t => t.Kind == TokenKind.StringLiteral);
            var charToken = tokens.First(t => t.Kind == TokenKind.CharLiteral);

            AssertToken(stringToken, TokenKind.StringLiteral, "\"Hi\\n\"");
            Assert.That(stringToken.LiteralValue, Is.EqualTo("Hi\n"));

            AssertToken(charToken, TokenKind.CharLiteral, "'\\n'");
            Assert.That(charToken.LiteralValue, Is.EqualTo('\n'));
        }

        [Test]
        public void FizzBuzzSample_StartsAndEndsWithExpectedTokens()
        {
            var source = @"
class FizzBuzz {
    construct() {
    }

    method run(): void {
        var i = 1;

        while (i <= 100) {
            var output = """";

            if (i % 3 == 0) {
                output = output + ""Fizz"";
            }

            if (i % 5 == 0) {
                output = output + ""Buzz"";
            }

            if (output == """") {
                // どちらにも当てはまらないときは数値を出力
                print(i);
            } else {
                // ""Fizz"", ""Buzz"", ""FizzBuzz"" を出力
                print(output);
            }

            i = i + 1;
        }
    }
}

// mainブロック想定
var fb = new FizzBuzz();
fb.run();
";

            var tokens = AloeLexer.Parse(source).ToArray();

            // 1. 先頭付近: class FizzBuzz {
            AssertToken(tokens[0], TokenKind.Keyword, "class");
            AssertToken(tokens[1], TokenKind.Identifier, "FizzBuzz");
            AssertToken(tokens[2], TokenKind.LeftBrace, "{");

            // 2. 中盤: method run(): void {
            AssertContainsSequence(
                tokens,
                new (TokenKind, string)[]
                {
                    (TokenKind.Keyword,    "method"),
                    (TokenKind.Identifier, "run"),
                    (TokenKind.LeftParen,  "("),
                    (TokenKind.RightParen, ")"),
                    (TokenKind.Colon,      ":"),
                    (TokenKind.Keyword,    "void"),
                    (TokenKind.LeftBrace,  "{"),
                },
                "method run(): void { のトークン列が見つからない"
            );

            // 3. 末尾1: var fb = new FizzBuzz();
            AssertContainsSequence(
                tokens,
                new (TokenKind, string)[]
                {
                    (TokenKind.Keyword,    "var"),
                    (TokenKind.Identifier, "fb"),
                    (TokenKind.Equals,     "="),
                    (TokenKind.Keyword,    "new"),
                    (TokenKind.Identifier, "FizzBuzz"),
                    (TokenKind.LeftParen,  "("),
                    (TokenKind.RightParen, ")"),
                    (TokenKind.Semicolon,  ";"),
                },
                "var fb = new FizzBuzz(); のトークン列が見つからない"
            );

            // 4. 末尾2: fb.run();
            AssertContainsSequence(
                tokens,
                new (TokenKind, string)[]
                {
                    (TokenKind.Identifier, "fb"),
                    (TokenKind.Dot,        "."),
                    (TokenKind.Identifier, "run"),
                    (TokenKind.LeftParen,  "("),
                    (TokenKind.RightParen, ")"),
                    (TokenKind.Semicolon,  ";"),
                },
                "fb.run(); のトークン列が見つからない"
            );

            // 5. 最後は必ず EOF
            Assert.That(tokens.Last().Kind, Is.EqualTo(TokenKind.EndOfFile));
        }
    }
}
