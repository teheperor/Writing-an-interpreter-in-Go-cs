namespace Monkey.Objects
{
    public class Boolean : IObject, IHashable
    {
        public bool Value { get; set; }

        public ObjectType Type => ObjectType.Boolean;

        public string Inspect() => Value.ToString().ToLower();

        public HashKey HashKey() => new HashKey(Type, Value ? 1ul : 0ul);
    }
}
