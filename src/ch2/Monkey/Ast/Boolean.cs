using Monkey.Tokens;

namespace Monkey.Ast
{
    public class Boolean : IExpression
    {
        public Token Token { get; set; }
        public bool Value { get; set; }

        public string TokenLiteral() => Token.Literal;

        public override string ToString() => Token.Literal;
    }
}
