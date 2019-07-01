using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monkey.Ast
{
    using ModifierFunc = Func<INode, INode>;

    //public static class Modify
    public static class Ast
    {
        public static INode Modify(INode node, ModifierFunc modifier)
        {
            switch (node)
            {
                case Program p:
                    p.Statements = p.Statements.
                        Select(statement => (IStatement)Modify(statement, modifier)).
                        ToList();
                    break;

                case ExpressionStatement es:
                    es.Expression = (IExpression)Modify(es.Expression, modifier);
                    break;

                case InfixExpression ie:
                    ie.Left = (IExpression)Modify(ie.Left, modifier);
                    ie.Right = (IExpression)Modify(ie.Right, modifier);
                    break;

                case PrefixExpression pe:
                    pe.Right = (IExpression)Modify(pe.Right, modifier);
                    break;

                case IndexExpression ie:
                    ie.Left = (IExpression)Modify(ie.Left, modifier);
                    ie.Index = (IExpression)Modify(ie.Index, modifier);
                    break;

                case IfExpression ie:
                    ie.Condition = (IExpression)Modify(ie.Condition, modifier);
                    ie.Consequence = (BlockStatement)Modify(ie.Consequence, modifier);
                    if (ie.Alternative != null)
                        ie.Alternative = (BlockStatement)Modify(ie.Alternative, modifier);
                    break;

                case BlockStatement bs:
                    bs.Statements = bs.Statements.Select(x => (IStatement)Modify(x, modifier)).ToList();
                    break;

                case ReturnStatement rs:
                    rs.ReturnValue = (IExpression)Modify(rs.ReturnValue, modifier);
                    break;

                case LetStatement ls:
                    ls.Value = (IExpression)Modify(ls.Value, modifier);
                    break;

                case FunctionLiteral fl:
                    fl.Parameters = fl.Parameters.Select(x => (Identifier)Modify(x, modifier)).ToList();
                    fl.Body = (BlockStatement)Modify(fl.Body, modifier);
                    break;

                case ArrayLiteral al:
                    al.Elements = al.Elements.Select(x => (IExpression)Modify(x, modifier)).ToList();
                    break;

                case HashLiteral hl:
                    hl.Pairs = hl.Pairs.
                        Select(x => ((IExpression)Modify(x.Key, modifier), (IExpression)Modify(x.Value, modifier))).
                        ToDictionary(x => x.Item1, x => x.Item2);
                    break;
            }

            return modifier(node);
        }
    }
}
