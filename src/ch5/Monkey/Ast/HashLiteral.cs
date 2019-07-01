using System.Collections.Generic;
using System.Linq;
using Monkey.Tokens;

namespace Monkey.Ast
{
    public class HashLiteral : IExpression
    {
        public Token Token { get; set; }
        public IDictionary<IExpression, IExpression> Pairs { get; set; }

        public string TokenLiteral() => Token.Literal;

        public override string ToString()
        {
            var pairs = Pairs.Select((key, value) => $"{key}:{value}");
            return $"{{{string.Join(", ", pairs)}}}";
        }
    }
}
