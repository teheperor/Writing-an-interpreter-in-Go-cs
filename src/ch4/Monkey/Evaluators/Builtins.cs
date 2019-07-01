using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Monkey.Objects;

namespace Monkey.Evaluators
{
    public static partial class Evaluator
    {
        private static readonly IImmutableDictionary<string, Builtin> builtins = new Dictionary<string, Builtin>
        {
            { "len", new Builtin { Fn = args => {
                var argsArr = args.ToImmutableArray();
                if (argsArr.Length  != 1)
                    return NewError($"wrong number of arguments. got={argsArr.Length}, want=1");

                var arg = argsArr[0];
                switch (arg)
                {
                    case Objects.Array a:
                        return new Integer { Value = (long)a.Elements.Count };
                    case Objects.String s:
                        return new Integer { Value = (long)s.Value.Length };
                    default:
                        return NewError($"argument to `len` not supported, got {arg.Type}");
                }
            } } },
            { "puts", new Builtin { Fn = args => {
                foreach(var arg in args)
                    Console.WriteLine(arg.Inspect());

                return NULL;
            } } },
            { "first", new Builtin { Fn = args => {
                var argsArr = args.ToImmutableArray();
                if (argsArr.Length  != 1)
                    return NewError($"wrong number of arguments. got={argsArr.Length}, want=1");
                if (argsArr[0].Type != ObjectType.Array)
                    return NewError($"argument to `first` must be Array, got {argsArr[0].Type}");

                var arr = (Objects.Array)argsArr[0];
                return arr.Elements.Count > 0 ? arr.Elements.First() : NULL;
            } } },
            { "last", new Builtin { Fn = args => {
                var argsArr = args.ToImmutableArray();
                if (argsArr.Length  != 1)
                    return NewError($"wrong number of arguments. got={argsArr.Length}, want=1");
                if (argsArr[0].Type != ObjectType.Array)
                    return NewError($"argument to `last` must be Array, got {argsArr[0].Type}");

                var arr = (Objects.Array)argsArr[0];
                return arr.Elements.Count > 0 ? arr.Elements.Last() : NULL;
            } } },
            { "rest", new Builtin { Fn = args => {
                var argsArr = args.ToImmutableArray();
                if (argsArr.Length  != 1)
                    return NewError($"wrong number of arguments. got={argsArr.Length}, want=1");
                if (argsArr[0].Type != ObjectType.Array)
                    return NewError($"argument to `rest` must be Array, got {argsArr[0].Type}");

                var arr = (Objects.Array)argsArr[0];
                if (arr.Elements.Count > 0)
                {
                    var newElements = new List<IObject>(arr.Elements.Skip(1));
                    return new Objects.Array { Elements = newElements };
                }

                return NULL;
            } } },
            { "push", new Builtin { Fn = args => {
                var argsArr = args.ToImmutableArray();
                if (argsArr.Length  != 2)
                    return NewError($"wrong number of arguments. got={argsArr.Length}, want=2");
                if (argsArr[0].Type != ObjectType.Array)
                    return NewError($"argument to `push` must be Array, got {argsArr[0].Type}");

                var arr = (Objects.Array)argsArr[0];

                var newElements = new List<IObject>(arr.Elements.Append(argsArr[1]));
                return new Objects.Array { Elements = newElements };
            } } },
        }.ToImmutableDictionary();
    }
}
