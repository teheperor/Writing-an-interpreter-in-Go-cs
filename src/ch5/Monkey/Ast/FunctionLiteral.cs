using System.Collections.Generic;
using System.Linq;
using Monkey.Tokens;

namespace Monkey.Ast
{
    public class FunctionLiteral : IExpression
    {
        public Token Token { get; set; }
        public IList<Identifier> Parameters { get; set; }
        public BlockStatement Body { get; set; }

        public string TokenLiteral() => Token.Literal;

        public override string ToString()
        {
            var @params = Parameters.Select(x => x.ToString());
            return $"{TokenLiteral()}({string.Join(", ", @params)}) {Body}";
        }
    }
}
