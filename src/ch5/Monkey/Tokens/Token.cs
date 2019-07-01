using System.Collections.Generic;
using System.Collections.Immutable;

namespace Monkey.Tokens
{
    public enum TokenType
    {
        Illegal,
        Eof,

        // Identifiers + literals
        Ident,  // add, foobar, x, y, ...
        Int,  // 1343456
        String,  // "foobar"

        // Operators
        Assign,
        Plus,
        Minus,
        Bang,
        Asterisk,
        Slash,

        LT,
        GT,

        Eq,
        NotEq,

        // Delimiters
        Comma,
        Semicolon,
        Colon,

        LParen,
        RParen,
        LBrace,
        RBrace,
        LBracket,
        RBracket,

        // Keywords
        Function,
        Let,
        True,
        False,
        If,
        Else,
        Return,
        Macro,
    }

    public class Token
    {
        private static readonly IImmutableDictionary<string, TokenType> keywords = new Dictionary<string, TokenType>
        {
            { "fn", TokenType.Function },
            { "let", TokenType.Let },
            { "true", TokenType.True },
            { "false", TokenType.False },
            { "if", TokenType.If },
            { "else", TokenType.Else },
            { "return", TokenType.Return },
            { "macro", TokenType.Macro },
        }.ToImmutableDictionary();

        public TokenType Type { get; set; }
        public string Literal { get; set; }

        public static TokenType LookupIdent(string ident) =>
            keywords.TryGetValue(ident, out var tok) ? tok : TokenType.Ident;

        public override string ToString() => $"{{Type:{Type} Literal:{Literal}}}";
    }
}
