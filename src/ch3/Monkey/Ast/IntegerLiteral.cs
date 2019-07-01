using Monkey.Tokens;

namespace Monkey.Ast
{
    public class IntegerLiteral : IExpression
    {
        public Token Token { get; set; }
        public long Value { get; set; }

        public string TokenLiteral() => Token.Literal;

        public override string ToString() => Token.Literal;
    }
}
