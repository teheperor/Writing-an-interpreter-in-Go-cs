using System;
using System.Collections.Generic;
using System.Linq;
using Monkey.Ast;
using Monkey.Objects;

namespace Monkey.Evaluators
{
    public static partial class Evaluator
    {
        public static void DefineMacros(Program program, Objects.Environment env)
        {
            var definitions = new List<int> { };

            foreach (var (statement, i) in program.Statements.Select((x, i) => (x, i)))
                if (IsMacroDefinition(statement))
                {
                    AddMacro(statement, env);
                    definitions.Add(i);
                }

            for (var i = definitions.Count - 1; i >= 0; i -= 1)
            {
                var definitionIndex = definitions[i];
                program.Statements =
                    program.Statements.Take(definitionIndex).
                    Concat(program.Statements.Skip(definitionIndex + 1)).
                    ToList();
            }
        }

        private static bool IsMacroDefinition(IStatement node)
        {
            var letStatement = node as LetStatement;
            if (letStatement == null)
                return false;

            if (!(letStatement.Value is MacroLiteral))
                return false;

            return true;
        }

        private static void AddMacro(IStatement stmt, Objects.Environment env)
        {
            var letStatement = (LetStatement)stmt;
            var macroLiteral = (MacroLiteral)letStatement.Value;

            var macro = new Macro
            {
                Parameters = macroLiteral.Parameters,
                Env = env,
                Body = macroLiteral.Body,
            };

            env.Set(letStatement.Name.Value, macro);
        }

        public static INode ExpandMacros(INode program, Objects.Environment env)
        {
            return Ast.Ast.Modify(program, node =>
            {
                var callExpression = node as CallExpression;
                if (callExpression == null)
                    return node;

                var macro = IsMacroCall(callExpression, env);
                if (macro == null)
                    return node;

                var args = QuoteArgs(callExpression);
                var evalEnv = ExtendMacroEnv(macro, args);

                var evaluated = Eval(macro.Body, evalEnv);

                var quote = evaluated as Quote;
                if (quote == null)
                    throw new NotSupportedException("we only support returning AST-nodes from macros");

                return quote.Node;
            });
        }

        private static Macro IsMacroCall(CallExpression exp, Objects.Environment env)
        {
            var identifier = exp.Function as Identifier;
            if (identifier == null)
                return null;

            var obj = env.Get(identifier.Value);
            if (obj == null)
                return null;

            var macro = obj as Macro;
            if (macro == null)
                return null;

            return macro;
        }

        private static IList<Quote> QuoteArgs(CallExpression exp) =>
            exp.Arguments.Select(a => new Quote { Node = a }).ToList();

        private static Objects.Environment ExtendMacroEnv(Macro macro, IEnumerable<Quote> args)
        {
            var extended = Objects.Environment.NewEnclosedEnvironment(macro.Env);

            foreach (var (param, arg) in macro.Parameters.Zip(args, (p, a) => (p, a)))
                extended.Set(param.Value, arg);

            return extended;
        }
    }
}
