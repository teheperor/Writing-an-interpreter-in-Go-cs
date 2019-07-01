using System.Collections.Generic;
using System.Text;

namespace Monkey.Ast
{
    public class Program : INode
    {
        public IList<IStatement> Statements { get; set; }

        public string TokenLiteral() =>
            Statements.Count > 0 ? Statements[0].TokenLiteral() : "";

        public override string ToString()
        {
            var @out = new StringBuilder();

            foreach (var s in Statements)
                @out.Append(s.ToString());

            return @out.ToString();
        }
    }
}
