using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Monkey.Ast;

namespace Monkey.Objects
{
    public class Macro : IObject
    {
        public IList<Identifier> Parameters { get; set; }
        public BlockStatement Body { get; set; }
        public Environment Env { get; set; }

        public ObjectType Type => ObjectType.Macro;

        public string Inspect()
        {
            var @params = Parameters.Select(p => p.ToString());
            return $"macro({string.Join(", ", @params)}) {{\n{Body}\n}}";
        }
    }
}
