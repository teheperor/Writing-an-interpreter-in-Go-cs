namespace Monkey.Objects
{
    public class Boolean : IObject
    {
        public bool Value { get; set; }

        public ObjectType Type => ObjectType.Boolean;

        public string Inspect() => Value.ToString().ToLower();
    }
}
