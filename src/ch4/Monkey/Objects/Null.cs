namespace Monkey.Objects
{
    public class Null : IObject
    {
        public ObjectType Type => ObjectType.Null;

        public string Inspect() => "null";
    }
}
