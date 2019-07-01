using System.Collections.Generic;
using System.Linq;
using Monkey.Tokens;

namespace Monkey.Ast
{
    public class ArrayLiteral : IExpression
    {
        public Token Token { get; set; }
        public IList<IExpression> Elements { get; set; }

        public string TokenLiteral() => Token.Literal;

        public override string ToString()
        {
            var elements = Elements.Select(x => x.ToString());
            return $"[{string.Join(", ", elements)}]";
        }
    }
}
