using Monkey.Tokens;

namespace Monkey.Ast
{
    public class ExpressionStatement : IStatement
    {
        public Token Token { get; set; }
        public IExpression Expression { get; set; }

        public string TokenLiteral() => Token.Literal;

        public override string ToString() => $"{Expression}";
    }
}
