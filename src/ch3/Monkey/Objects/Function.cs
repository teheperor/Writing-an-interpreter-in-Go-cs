using System.Collections.Generic;
using System.Linq;
using Monkey.Ast;

namespace Monkey.Objects
{
    public class Function : IObject
    {
        public IList<Identifier> Parameters { get; set; }
        public BlockStatement Body { get; set; }
        public Environment Env { get; set; }

        public ObjectType Type => ObjectType.Function;

        public string Inspect()
        {
            var @params = Parameters.Select(x => x.ToString());
            return $"fn({string.Join(", ", @params)}) {{\n{Body}\n}}";
        }
    }
}
