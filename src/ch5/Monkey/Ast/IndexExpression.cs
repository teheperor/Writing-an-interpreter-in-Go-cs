using Monkey.Tokens;

namespace Monkey.Ast
{
    public class IndexExpression : IExpression
    {
        public Token Token { get; set; }
        public IExpression Left { get; set; }
        public IExpression Index { get; set; }

        public string TokenLiteral() => Token.Literal;

        public override string ToString() => $"({Left}[{Index}])";
    }
}
