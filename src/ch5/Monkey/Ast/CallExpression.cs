using System.Collections.Generic;
using System.Linq;
using Monkey.Tokens;

namespace Monkey.Ast
{
    public class CallExpression : IExpression
    {
        public Token Token { get; set; }
        public IExpression Function { get; set; }
        public IList<IExpression> Arguments { get; set; }

        public string TokenLiteral() => Token.Literal;

        public override string ToString()
        {
            var args = Arguments.Select(x => x.ToString());
            return $"{Function}({string.Join(", ", args)})";
        }
    }
}
