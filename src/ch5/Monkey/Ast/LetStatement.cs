using Monkey.Tokens;

namespace Monkey.Ast
{
    public class LetStatement : IStatement
    {
        public Token Token { get; set; }
        public Identifier Name { get; set; }
        public IExpression Value { get; set; }

        public string TokenLiteral() => Token.Literal;

        public override string ToString() => $"{TokenLiteral()} {Name} = {Value};";
    }
}
