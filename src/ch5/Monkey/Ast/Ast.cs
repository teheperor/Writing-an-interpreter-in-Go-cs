namespace Monkey.Ast
{
    public interface INode
    {
        string TokenLiteral();
    }

    public interface IStatement : INode { }

    public interface IExpression : INode { }
}
