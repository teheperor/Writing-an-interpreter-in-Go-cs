namespace Monkey.Objects
{
    public class Integer : IObject, IHashable
    {
        public long Value { get; set; }

        public ObjectType Type => ObjectType.Integer;

        public string Inspect() => Value.ToString();

        public HashKey HashKey() => new HashKey(Type, (ulong)Value);
    }
}
