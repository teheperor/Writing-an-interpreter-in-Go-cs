using Monkey.Ast;

namespace Monkey.Objects
{
    public class Quote : IObject
    {
        public INode Node { get; set; }

        public ObjectType Type => ObjectType.Quote;

        public string Inspect() => $"QUOTE({Node})";
    }
}
