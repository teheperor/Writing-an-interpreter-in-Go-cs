using System;
using System.Collections.Generic;
using System.Text;
using Monkey.Objects;
using Xunit;

namespace Monkey.Tests.Evaluators
{
    public partial class EvaluatorTest
    {
        [Fact]
        public void TestQuote()
        {
            var tests = new[]
            {
                ("quote(5)", "5"),
                ("quote(5 + 8)", "(5 + 8)"),
                ("quote(foobar)", "foobar"),
                ("quote(foobar + barfoo)", "(foobar + barfoo)"),
            };

            foreach (var (input, expected) in tests)
            {
                var evaluated = TestEval(input);
                Assert.IsType<Quote>(evaluated);
                var quote = (Quote)evaluated;

                Assert.NotNull(quote.Node);

                Assert.Equal(expected, quote.Node.ToString());
            }
        }

        [Fact]
        public void TestQuoteUnquote()
        {
            var tests = new[]
            {
                ("quote(unquote(4))", "4"),
                ("quote(unquote(4 + 4))", "8"),
                ("quote(8 + unquote(4 + 4))", "(8 + 8)"),
                ("quote(unquote(4 + 4) + 8)", "(8 + 8)"),
                (@"let foobar = 8;
                    quote(foobar)", 
                "foobar"),
                (@"let foobar = 8; 
                    quote(unquote(foobar))", 
                "8"),
                ("quote(unquote(true))",
                "true"),
                ("quote(unquote(true == false))",
                "false"),
                ("quote(unquote(quote(4 + 4)))",
                "(4 + 4)"),
                (@"let quotedInfixExpression = quote(4 + 4);
                    quote(unquote(4 + 4) + unquote(quotedInfixExpression))",
                "(8 + (4 + 4))"),
            };

            foreach (var (input, expected) in tests)
            {
                var evaluated = TestEval(input);
                Assert.IsType<Quote>(evaluated);
                var quote = (Quote)evaluated;

                Assert.NotNull(quote.Node);

                Assert.Equal(expected, quote.Node.ToString());
            }
        }
    }
}
