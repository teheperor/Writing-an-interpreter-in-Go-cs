namespace Monkey.Objects
{
    public enum ObjectType
    {
        Null,
        Error,
        Integer,
        Boolean,
        ReturnValue,
        Function,
    }

    public interface IObject
    {
        ObjectType Type { get; }
        string Inspect();
    }
}
