namespace Monkey.Objects
{
    public enum ObjectType
    {
        Null,
        Error,
        Integer,
        Boolean,
        String,
        ReturnValue,
        Function,
        Builtin,
        Array,
        Hash,
        Quote,
        Macro,
    }

    public interface IObject
    {
        ObjectType Type { get; }
        string Inspect();
    }
}
