using Monkey.Tokens;

namespace Monkey.Ast
{
    public class StringLiteral : IExpression
    {
        public Token Token { get; set; }
        public string Value { get; set; }

        public string TokenLiteral() => Token.Literal;

        public override string ToString() => Token.Literal;
    }
}
