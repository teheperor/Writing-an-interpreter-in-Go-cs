using System.Collections.Generic;
using System.Linq;
using Monkey.Ast;
using Monkey.Objects;

namespace Monkey.Evaluators
{
    public static class Evaluator
    {
        public static readonly Null NULL = new Null { };
        private static readonly Objects.Boolean TRUE = new Objects.Boolean { Value = true };
        private static readonly Objects.Boolean FALSE = new Objects.Boolean { Value = false };

        public static IObject Eval(INode node, Environment env)
        {
            switch (node)
            {
                case Program p: return EvalProgram(p, env);
                case BlockStatement bs: return EvalBlockStatement(bs, env);
                case ExpressionStatement es: return Eval(es.Expression, env);
                case ReturnStatement rs:
                    {
                        var val = Eval(rs.ReturnValue, env);
                        if (IsError(val))
                            return val;
                        return new ReturnValue { Value = val };
                    }
                case LetStatement ls:
                    {
                        var val = Eval(ls.Value, env);
                        if (IsError(val))
                            return val;
                        env.Set(ls.Name.Value, val);
                        break;
                    }
                case IntegerLiteral il: return new Integer { Value = il.Value };
                case Ast.Boolean b: return NativeToBooleanObject(b.Value);
                case PrefixExpression pe:
                    {
                        var right = Eval(pe.Right, env);
                        if (IsError(right))
                            return right;
                        return EvalPrefixExpression(pe.Operator, right);
                    }
                case InfixExpression ie:
                    {
                        var left = Eval(ie.Left, env);
                        if (IsError(left))
                            return left;

                        var right = Eval(ie.Right, env);
                        if (IsError(right))
                            return right;

                        return EvalInfixExpression(ie.Operator, left, right);
                    }
                case IfExpression ie: return EvalIfExpression(ie, env);
                case Identifier i: return EvalIdentifier(i, env);
                case FunctionLiteral fl:
                    {
                        var @params = fl.Parameters;
                        var body = fl.Body;
                        return new Function { Parameters = @params, Env = env, Body = body };
                    }
                case CallExpression ce:
                    {
                        var function = Eval(ce.Function, env);
                        if (IsError(function))
                            return function;
                        var args = EvalExpressions(ce.Arguments, env);
                        if (args.Count == 1 && IsError(args[0]))
                            return args[0];

                        return ApplyFunction(function, args);
                    }
            };

            return null;
        }

        private static IObject EvalProgram(Program program, Environment env)
        {
            IObject result = null;

            foreach (var statement in program.Statements)
            {
                result = Eval(statement, env);

                switch (result)
                {
                    case ReturnValue rv: return rv.Value;
                    case Error e: return e;
                }
            }

            return result;
        }

        private static IObject EvalBlockStatement(BlockStatement block, Environment env)
        {
            IObject result = null;

            foreach (var statement in block.Statements)
            {
                result = Eval(statement, env);

                if (result != null)
                {
                    var rt = result.Type;
                    if (rt == ObjectType.ReturnValue || rt == ObjectType.Error)
                        return result;
                }
            }

            return result;
        }

        private static Objects.Boolean NativeToBooleanObject(bool input) => input ? TRUE : FALSE;

        private static IObject EvalPrefixExpression(string @operator, IObject right)
        {
            switch (@operator)
            {
                case "!": return EvalBangOperatorExpression(right);
                case "-": return EvalMinusOperatorExpression(right);
                default: return NewError($"unknown operator: {@operator}{right.Type}");
            }
        }

        private static IObject EvalInfixExpression(string @operator, IObject left, IObject right)
        {
            switch (@operator)
            {
                case string _ when left.Type == ObjectType.Integer && right.Type == ObjectType.Integer:
                    return EvalIntegerInfixExpression(@operator, left, right);
                case "==": return NativeToBooleanObject(left == right);
                case "!=": return NativeToBooleanObject(left != right);
                case string _ when left.Type != right.Type:
                    return NewError($"type mismatch: {left.Type} {@operator} {right.Type}");
                default:
                    return NewError($"unknown operator: {left.Type} {@operator} {right.Type}");
            }
        }

        private static IObject EvalBangOperatorExpression(IObject right)
        {
            return right switch
            {
                Objects.Boolean b when b == TRUE => FALSE,
                Objects.Boolean b when b == FALSE => TRUE,
                Null _ => TRUE,
                _ => FALSE,
            };
        }

        private static IObject EvalMinusOperatorExpression(IObject right)
        {
            if (right.Type != ObjectType.Integer)
                return NewError($"unknown operator: -{right.Type}");

            var value = ((Integer)right).Value;
            return new Integer { Value = -value };
        }

        private static IObject EvalIntegerInfixExpression(string @operator, IObject left, IObject right)
        {
            var leftVal = ((Integer)left).Value;
            var rightVal = ((Integer)right).Value;

            return @operator switch
            {
                "+" => new Integer { Value = leftVal + rightVal },
                "-" => new Integer { Value = leftVal - rightVal },
                "*" => new Integer { Value = leftVal * rightVal },
                "/" => new Integer { Value = leftVal / rightVal },
                "<" => NativeToBooleanObject(leftVal < rightVal),
                ">" => NativeToBooleanObject(leftVal > rightVal),
                "==" => NativeToBooleanObject(leftVal == rightVal),
                "!=" => NativeToBooleanObject(leftVal != rightVal),
                _ => (IObject)NewError($"unknown operator: {left.Type} {@operator} {right.Type}"),
            };
        }

        private static IObject EvalIfExpression(IfExpression ie, Environment env)
        {
            var condition = Eval(ie.Condition, env);
            if (IsError(condition))
                return condition;

            if (IsTruthy(condition))
                return Eval(ie.Consequence, env);
            else if (ie.Alternative != null)
                return Eval(ie.Alternative, env);
            else
                return NULL;
        }

        private static IObject EvalIdentifier(Identifier node, Environment env)
        {
            var val = env.Get(node.Value);
            if (val == null)
                return NewError($"identifier not found: {node.Value}");

            return val;
        }

        private static bool IsTruthy(IObject obj)
        {
            return obj switch
            {
                Null _ => false,
                Objects.Boolean b when b == TRUE => true,
                Objects.Boolean b when b == FALSE => false,
                _ => true,
            };
        }

        private static Error NewError(string message) => new Error { Message = message };

        private static bool IsError(IObject obj) => obj?.Type == ObjectType.Error;

        private static IList<IObject> EvalExpressions(IEnumerable<IExpression> exps, Environment env)
        {
            var result = new List<IObject>();

            foreach (var e in exps)
            {
                var evaluated = Eval(e, env);
                if (IsError(evaluated))
                    return new List<IObject> { evaluated };
                result.Add(evaluated);
            }

            return result;
        }

        private static IObject ApplyFunction(IObject fn, IEnumerable<IObject> args)
        {
            var function = fn as Function;
            if (function == null)
                NewError($"not a function: {fn.Type}");

            var extendedEnv = ExtendFunctionEnv(function, args);
            var evaluated = Eval(function.Body, extendedEnv);
            return UnwrapReturnValue(evaluated);
        }

        private static Environment ExtendFunctionEnv(Function fn, IEnumerable<IObject> args)
        {
            var env = Environment.NewEnclosedEnvironment(fn.Env);

            foreach (var (param, arg) in fn.Parameters.Zip(args, (p, a) => (p, a)))
                env.Set(param.Value, arg);

            return env;
        }

        private static IObject UnwrapReturnValue(IObject obj) =>
            obj is ReturnValue rv ? rv.Value : obj;
    }
}
