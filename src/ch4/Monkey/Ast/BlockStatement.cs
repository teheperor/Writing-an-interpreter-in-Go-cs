using System.Collections.Generic;
using System.Text;
using Monkey.Tokens;

namespace Monkey.Ast
{
    public class BlockStatement : IStatement
    {
        public Token Token { get; set; }
        public IList<IStatement> Statements { get; set; }

        public string TokenLiteral() => Token.Literal;

        public override string ToString()
        {
            var @out = new StringBuilder();

            foreach (var s in Statements)
                @out.Append(s.ToString());

            return @out.ToString();
        }
    }
}
