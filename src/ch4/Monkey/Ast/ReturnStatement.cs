using Monkey.Tokens;

namespace Monkey.Ast
{
    public class ReturnStatement : IStatement
    {
        public Token Token { get; set; }
        public IExpression ReturnValue { get; set; }

        public string TokenLiteral() => Token.Literal;

        public override string ToString() => $"{TokenLiteral()} {ReturnValue};";
    }
}
