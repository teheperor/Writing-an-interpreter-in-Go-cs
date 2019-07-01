using Monkey.Tokens;

namespace Monkey.Lexers
{
    public class Lexer
    {
        private readonly string input;
        private int position;
        private int readPosition;
        private char ch;

        public Lexer(string input)
        {
            this.input = input;
            ReadChar();
        }

        public Token NextToken()
        {
            Token tok;

            SkipWhitespace();

            switch (ch)
            {
                case '=':
                    if (PeekChar() == '=')
                    {
                        var ch = this.ch;
                        ReadChar();
                        var literal = $"{ch}{this.ch}";
                        tok = new Token { Type = TokenType.Eq, Literal = literal };
                    }
                    else
                        tok = NewToken(TokenType.Assign, ch);
                    break;
                case '+': tok = NewToken(TokenType.Plus, ch); break;
                case '-': tok = NewToken(TokenType.Minus, ch); break;
                case '!':
                    if (PeekChar() == '=')
                    {
                        var ch = this.ch;
                        ReadChar();
                        var literal = $"{ch}{this.ch}";
                        tok = new Token { Type = TokenType.NotEq, Literal = literal };
                    }
                    else
                        tok = NewToken(TokenType.Bang, ch);
                    break;
                case '/': tok = NewToken(TokenType.Slash, ch); break;
                case '*': tok = NewToken(TokenType.Asterisk, ch); break;
                case '<': tok = NewToken(TokenType.LT, ch); break;
                case '>': tok = NewToken(TokenType.GT, ch); break;
                case ';': tok = NewToken(TokenType.Semicolon, ch); break;
                case ':': tok = NewToken(TokenType.Colon, ch); break;
                case ',': tok = NewToken(TokenType.Comma, ch); break;
                case '(': tok = NewToken(TokenType.LParen, ch); break;
                case ')': tok = NewToken(TokenType.RParen, ch); break;
                case '"':
                    tok = new Token { Type = TokenType.String, Literal = ReadString() };
                    break;
                case '{': tok = NewToken(TokenType.LBrace, ch); break;
                case '}': tok = NewToken(TokenType.RBrace, ch); break;
                case '[': tok = NewToken(TokenType.LBracket, ch); break;
                case ']': tok = NewToken(TokenType.RBracket, ch); break;
                case (char)0: tok = new Token { Type = TokenType.Eof, Literal = "" }; break;
                default:
                    if (IsLetter(ch))
                    {
                        var literal = ReadIdentifier();
                        var type = Token.LookupIdent(literal);
                        tok = new Token { Type = type, Literal = literal };
                        return tok;
                    }
                    else if (IsDigit(ch))
                    {
                        tok = new Token { Type = TokenType.Int, Literal = ReadNumber() };
                        return tok;
                    }
                    else
                        tok = NewToken(TokenType.Illegal, ch);
                    break;
            }

            ReadChar();
            return tok;
        }

        private void SkipWhitespace()
        {
            while (ch == ' ' || ch == '\t' || ch == '\n' || ch == '\r')
                ReadChar();
        }

        private void ReadChar()
        {
            ch = readPosition >= input.Length ? (char)0 : input[readPosition];
            position = readPosition;
            readPosition++;
        }

        private char PeekChar() =>
            readPosition >= input.Length ? (char)0 : input[readPosition];

        private string ReadIdentifier()
        {
            var position = this.position;
            while (IsLetter(ch))
                ReadChar();
            return input.Substring(position, this.position - position);
        }

        private string ReadNumber()
        {
            var position = this.position;
            while (IsDigit(ch))
                ReadChar();
            return input.Substring(position, this.position - position);
        }

        private string ReadString()
        {
            var position = this.position + 1;
            while (true)
            {
                ReadChar();
                if (ch == '"' || ch == 0)
                    break;
            }
            return input.Substring(position, this.position - position);
        }

        private static bool IsLetter(char ch) =>
            'a' <= ch && ch <= 'z' || 'A' <= ch && ch <= 'Z' || ch == '_';

        private static bool IsDigit(char ch) => '0' <= ch && ch <= '9';

        private Token NewToken(TokenType tokenType, char ch) =>
            new Token { Type = tokenType, Literal = ch.ToString() };
    }
}
