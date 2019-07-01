namespace Monkey.Objects
{
    public class Error : IObject
    {
        public string Message { get; set; }

        public ObjectType Type => ObjectType.Error;

        public string Inspect() => $"ERROR: {Message}";
    }
}
