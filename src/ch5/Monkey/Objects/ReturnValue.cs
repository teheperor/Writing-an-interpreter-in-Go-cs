namespace Monkey.Objects
{
    public class ReturnValue : IObject
    {
        public IObject Value { get; set; }

        public ObjectType Type => ObjectType.ReturnValue;

        public string Inspect() => Value.Inspect();
    }
}
