namespace Monkey.Objects
{
    public class Integer : IObject
    {
        public long Value { get; set; }

        public ObjectType Type => ObjectType.Integer;

        public string Inspect() => Value.ToString();
    }
}
