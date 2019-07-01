using Monkey.Tokens;

namespace Monkey.Ast
{
    public class PrefixExpression : IExpression
    {
        public Token Token { get; set; }
        public string Operator { get; set; }
        public IExpression Right { get; set; }

        public string TokenLiteral() => Token.Literal;

        public override string ToString() => $"({Operator}{Right})";
    }
}
