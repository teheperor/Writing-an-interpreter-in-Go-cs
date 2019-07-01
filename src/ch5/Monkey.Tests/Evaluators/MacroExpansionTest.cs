using System.Collections.Generic;
using System.Text;
using Xunit;
using Monkey.Objects;
using Monkey.Ast;
using Monkey.Lexers;
using Monkey.Parsers;
using Monkey.Evaluators;

namespace Monkey.Tests.Evaluators
{
    public class MacroExpansionTest
    {
        [Fact]
        public void TestDefineMacros()
        {
            var input = @"
            let number = 1;
            let function = fn(x, y) { x + y };
            let mymacro = macro(x, y) { x + y; };
            ";

            var env = new Environment();
            var program = TestParseProgram(input);

            Evaluator.DefineMacros(program, env);

            Assert.Equal(2, program.Statements.Count);

            Assert.Null(env.Get("number"));
            Assert.Null(env.Get("function"));

            var obj = env.Get("mymacro");
            Assert.NotNull(obj);

            Assert.IsType<Macro>(obj);
            var macro = (Macro)obj;

            Assert.Equal(2, macro.Parameters.Count);

            Assert.Equal("x", macro.Parameters[0].ToString());
            Assert.Equal("y", macro.Parameters[1].ToString());

            var expectedBody = "(x + y)";

            Assert.Equal(expectedBody, macro.Body.ToString());
        }

        [Fact]
        public void TestExpandMacros()
        {
            var tests = new[]
            {
                (@"
                let infixExpression = macro() { quote(1 + 2); };

                infixExpression();
                ",
                "(1 + 2)"),
                (@"
                let reverse = macro(a, b) { quote(unquote(b) - unquote(a)); };

                reverse(2 + 2, 10 - 5);
                ",
                "(10 - 5) - (2 + 2)"),
                (@"
                let unless = macro(condition, consequence, alternative) {
                    quote(if (!(unquote(condition))) {
                        unquote(consequence);
                    } else {
                        unquote(alternative);
                    });
                };

                unless(10 > 5, puts(""not greater""), puts(""greater""));
                ",
                @"if (!(10 > 5)) { puts(""not greater"") } else { puts(""greater"") }"),
            };

            foreach (var (tinput, texpected) in tests)
            {
                var expected = TestParseProgram(texpected);
                var program = TestParseProgram(tinput);

                var env = new Environment();
                Evaluator.DefineMacros(program, env);
                var expanded = Evaluator.ExpandMacros(program, env);

                Assert.Equal(expected.ToString(), expanded.ToString());
            }
        }

        private static Program TestParseProgram(string input)
        {
            var l = new Lexer(input);
            var p = new Parser(l);
            return p.ParseProgram();
        }

    }
}
