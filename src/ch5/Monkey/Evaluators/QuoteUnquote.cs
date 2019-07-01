using Monkey.Ast;
using Monkey.Objects;
using Monkey.Tokens;

namespace Monkey.Evaluators
{
    public static partial class Evaluator
    {
        private static IObject Quote(INode node, Environment env)
        {
            node = EvalUnquoteCalls(node, env);
            return new Quote { Node = node };
        }

        private static INode EvalUnquoteCalls(INode quoted, Environment env)
        {
            return Ast.Ast.Modify(quoted, node =>
            {
                if (!IsUnquoteCall(node))
                    return node;

                var call = node as CallExpression;
                if (call == null)
                    return node;

                if (call.Arguments.Count != 1)
                    return node;

                var unquoted = Eval(call.Arguments[0], env);
                return ConvertObjectToAstNode(unquoted);
            });
        }

        private static bool IsUnquoteCall(INode node)
        {
            var callExpression = node as CallExpression;
            if (callExpression == null)
                return false;

            return callExpression.Function.TokenLiteral() == "unquote";
        }

        private static INode ConvertObjectToAstNode(IObject obj)
        {
            switch (obj)
            {
                case Integer i:
                    {
                        var t = new Token { Type = TokenType.Int, Literal = $"{i.Value}" };
                        return new IntegerLiteral { Token = t, Value = i.Value };
                    }

                case Objects.Boolean b:
                    {
                        var t = b.Value ?
                            new Token { Type = TokenType.True, Literal = "true" } :
                            new Token { Type = TokenType.False, Literal = "false" };
                        return new Ast.Boolean { Token = t, Value = b.Value };
                    }

                case Quote q:
                    return q.Node;

                default:
                    return null;
            }
        }
    }
}
