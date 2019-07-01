using Monkey.Tokens;

namespace Monkey.Ast
{
    public class IfExpression : IExpression
    {
        public Token Token { get; set; }
        public IExpression Condition { get; set; }
        public BlockStatement Consequence { get; set; }
        public BlockStatement Alternative { get; set; }

        public string TokenLiteral() => Token.Literal;

        public override string ToString() =>
            $"if {Condition} {Consequence}" + (Alternative != null ? $"else {Alternative}" : "");
    }
}
