using Monkey.Evaluators;
using Monkey.Lexers;
using Monkey.Objects;
using Monkey.Parsers;
using Xunit;

namespace Monkey.Tests.Evaluators
{
    public class EvaluatorTest
    {
        [Fact]
        public void TestEvalIntegerExpression()
        {
            var tests = new[]
            {
                ("5", 5L),
                ("10", 10L),
                ("-5", -5L),
                ("-10", -10L),
                ("5 + 5 + 5 + 5 - 10", 10L),
                ("2 * 2 * 2 * 2 * 2", 32L),
                ("-50 + 100 + -50", 0L),
                ("5 * 2 + 10", 20L),
                ("5 + 2 * 10", 25L),
                ("20 + 2 * -10", 0L),
                ("50 / 2 * 2 + 10", 60L),
                ("2 * (5 + 10)", 30L),
                ("3 * 3 * 3 + 10", 37L),
                ("3 * (3 * 3) + 10", 37L),
                ("(5 + 10 * 2 + 15 / 3) * 2 + -10", 50L),
            };

            foreach (var (input, expected) in tests)
            {
                var evaluated = TestEval(input);
                TestIntegerObject(evaluated, expected);
            }
        }

        [Fact]
        public void TestEvalBooleanExpression()
        {
            var tests = new[]
            {
                ("true", true),
                ("false", false),
                ("1 < 2", true),
                ("1 > 2", false),
                ("1 < 1", false),
                ("1 > 1", false),
                ("1 == 1", true),
                ("1 != 1", false),
                ("1 == 2", false),
                ("1 != 2", true),
                ("true == true", true),
                ("false == false", true),
                ("true == false", false),
                ("true != false", true),
                ("false != true", true),
                ("(1 < 2) == true", true),
                ("(1 < 2) == false", false),
                ("(1 > 2) == true", false),
                ("(1 > 2) == false", true),
            };

            foreach (var (input, expected) in tests)
            {
                var evaluated = TestEval(input);
                TestBooleanObject(evaluated, expected);
            }
        }

        [Fact]
        public void TestBangOperator()
        {
            var tests = new[]
            {
                ("!true", false),
                ("!false", true),
                ("!5", false),
                ("!!true", true),
                ("!!false", false),
                ("!!5", true),
            };

            foreach (var (input, expected) in tests)
            {
                var evaluated = TestEval(input);
                TestBooleanObject(evaluated, expected);
            }
        }

        [Fact]
        public void TestIfElseExpression()
        {
            var tests = new (string, long?)[]
            {
                ("if (true) { 10 }", 10L),
                ("if (false) { 10 }", null),
                ("if (1) { 10 }", 10L),
                ("if (1 < 2) { 10 }", 10L),
                ("if (1 > 2) { 10 }", null),
                ("if (1 > 2) { 10 } else { 20 }", 20L),
                ("if (1 < 2) { 10 } else { 20 }", 10L),
            };

            foreach (var (input, expected) in tests)
            {
                var evaluated = TestEval(input);
                if (expected.HasValue)
                    TestIntegerObject(evaluated, expected.Value);
                else
                    TestNullObject(evaluated);
            }
        }

        [Fact]
        public void TestReturnStatements()
        {
            var tests = new[]
            {
                ("return 10;", 10L),
                ("return 10; 9;", 10L),
                ("return 2 * 5; 9;", 10L),
                ("9; return 2 * 5; 9;", 10L),
                (@"
if (10 > 1) {
  if (10 > 1) {
    return 10;
  }

  return 1;
}
", 10L),
            };

            foreach (var (input, expected) in tests)
            {
                var evaluated = TestEval(input);
                TestIntegerObject(evaluated, expected);
            }
        }

        [Fact]
        public void TestErrorHandling()
        {
            var tests = new[]
            {
                ("5 + true;",
                "type mismatch: Integer + Boolean"),
                ("5 + true; 5;",
                "type mismatch: Integer + Boolean"),
                ("-true",
                "unknown operator: -Boolean"),
                ("true + false",
                "unknown operator: Boolean + Boolean"),
                ("5; true + false; 5",
                "unknown operator: Boolean + Boolean"),
                ("if (10 > 1) { true + false; }",
                "unknown operator: Boolean + Boolean"),
                (@"
if (10 > 1) {
  if (10 > 1) {
    return true + false;
  }

  return 1;
}
",
                "unknown operator: Boolean + Boolean"),
                ("foobar",
                "identifier not found: foobar"),
            };

            foreach (var (input, expected) in tests)
            {
                var evaluated = TestEval(input);

                Assert.IsType<Error>(evaluated);
                var errObj = (Error)evaluated;
                Assert.Equal(expected, errObj.Message);
            }
        }

        [Fact]
        public void TestLetStatement()
        {
            var tests = new[]
            {
                ("let a = 5; a;", 5L),
                ("let a = 5 * 5; a;", 25L),
                ("let a = 5; let b = a; b;", 5L),
                ("let a = 5; let b = a; let c = a + b + 5; c;", 15L),
            };

            foreach (var (input, expected) in tests)
                TestIntegerObject(TestEval(input), expected);
        }

        [Fact]
        public void TestFunctionObject()
        {
            var input = "fn(x) { x + 2; };";

            var evaluated = TestEval(input);
            Assert.IsType<Function>(evaluated);
            var fn = (Function)evaluated;

            Assert.Equal(1, fn.Parameters.Count);
            Assert.Equal("x", fn.Parameters[0].ToString());

            var expectedBody = "(x + 2)";

            Assert.Equal(expectedBody, fn.Body.ToString());
        }

        [Fact]
        public void TestFunctionApplication()
        {
            var tests = new[]
            {
                ("let identity = fn(x) { x; }; identity(5);", 5L),
                ("let identity = fn(x) { return x; }; identity(5);", 5L),
                ("let double = fn(x) { x * 2; }; double(5);", 10L),
                ("let add = fn(x, y) { x + y; }; add(5, 5);", 10L),
                ("let add = fn(x, y) { x + y; }; add(5 + 5, add(5, 5));", 20L),
                ("fn(x) { x; }(5)", 5L),
            };

            foreach (var (input, expected) in tests)
                TestIntegerObject(TestEval(input), expected);
        }

        [Fact]
        public void TestClosure()
        {
            var input = @"
let newAdder = fn(x) {
  fn(y) { x + y };
};

let addTwo = newAdder(2);
addTwo(2);";

            TestIntegerObject(TestEval(input), 4L);
        }

        private IObject TestEval(string input)
        {
            var l = new Lexer(input);
            var p = new Parser(l);
            var program = p.ParseProgram();
            var env = new Environment();

            return Evaluator.Eval(program, env);
        }

        private void TestIntegerObject(IObject obj, long expected)
        {
            Assert.IsType<Integer>(obj);
            var result = (Integer)obj;
            Assert.Equal(expected, result.Value);
        }

        private void TestBooleanObject(IObject obj, bool expected)
        {
            Assert.IsType<Monkey.Objects.Boolean>(obj);
            var result = (Monkey.Objects.Boolean)obj;
            Assert.Equal(expected, result.Value);
        }

        private void TestNullObject(IObject obj) => Assert.Equal(Evaluator.NULL, obj);
    }
}
