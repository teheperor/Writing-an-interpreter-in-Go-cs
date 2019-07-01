using System.Linq;
using Monkey.Evaluators;
using Monkey.Lexers;
using Monkey.Objects;
using Monkey.Parsers;
using Xunit;

namespace Monkey.Tests.Evaluators
{
    public partial class EvaluatorTest
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
                (@"""Hello"" - ""World""",
                "unknown operator: String - String"),
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
                (@"{""name"": ""Monkey""}[fn(x) { x }];",
                "unusable as hash key: Function"),
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

        [Fact]
        public void TestStringLiteral()
        {
            var input = @"""Hello World!""";

            var evaluated = TestEval(input);
            Assert.IsType<String>(evaluated);
            var str = (String)evaluated;

            Assert.Equal("Hello World!", str.Value);
        }

        [Fact]
        public void TestStringConcatenation()
        {
            var input = @"""Hello"" + "" "" + ""World!""";

            var evaluated = TestEval(input);
            Assert.IsType<String>(evaluated);
            var str = (String)evaluated;

            Assert.Equal("Hello World!", str.Value);
        }


        [Fact]
        public void TestBuiltinFunctions()
        {
            var tests = new (string, object)[]
            {
                (@"len("""")", 0L),
                (@"len(""four"")", 4L),
                (@"len(""hello world"")", 11L),
                (@"len(1)", "argument to `len` not supported, got Integer"),
                (@"len(""one"", ""two"")", "wrong number of arguments. got=2, want=1"),
                ("len([1, 2, 3])", 3L),
                ("len([])", 0L),
                (@"puts(""hello"", ""world!"")", null),
                ("first([1, 2, 3])", 1L),
                ("first([])", null),
                ("first(1)", "argument to `first` must be Array, got Integer"),
                ("last([1, 2, 3])", 3L),
                ("last([])", null),
                ("last(1)", "argument to `last` must be Array, got Integer"),
                ("rest([1, 2, 3])", new[] { 2L, 3L }),
                ("rest([])", null),
                ("push([], 1)", new[] { 1L }),
                ("push(1, 1)", "argument to `push` must be Array, got Integer"),
            };

            foreach (var (input, expected) in tests)
            {
                var evaluated = TestEval(input);

                switch (expected)
                {
                    case long l:
                        TestIntegerObject(evaluated, l);
                        break;
                    case string s:
                        Assert.IsType<Error>(evaluated);
                        var errObj = (Error)evaluated;
                        Assert.Equal(s, errObj.Message);
                        break;
                    case long[] la:
                        Assert.IsType<Array>(evaluated);
                        var array = (Array)evaluated;

                        Assert.Equal(la.Length, array.Elements.Count);

                        foreach (var (expectedElem, element) in la.Zip(array.Elements, (ee, e) => (ee, e)))
                            TestIntegerObject(element, expectedElem);
                        break;
                    case null:
                        TestNullObject(evaluated);
                        break;
                }
            }
        }

        [Fact]
        public void TestArrayLiterals()
        {
            var input = "[1, 2 * 2, 3 + 3]";

            var evaluated = TestEval(input);
            Assert.IsType<Array>(evaluated);
            var result = (Array)evaluated;

            Assert.Equal(3, result.Elements.Count);

            TestIntegerObject(result.Elements[0], 1L);
            TestIntegerObject(result.Elements[1], 4L);
            TestIntegerObject(result.Elements[2], 6L);
        }

        [Fact]
        public void TestArrayIndexExpressions()
        {
            var tests = new (string, object)[] {
                ("[1, 2, 3][0]", 1L),
                ("[1, 2, 3][1]", 2L),
                ("[1, 2, 3][2]", 3L),
                ("let i = 0; [1][i];", 1L),
                ("[1, 2, 3][1 + 1]", 3L),
                ("let myArray = [1, 2, 3]; myArray[2];", 3L),
                ("let myArray = [1, 2, 3]; myArray[0] + myArray[1] + myArray[2];", 6L),
                ("let myArray = [1, 2, 3]; let i = myArray[0]; myArray[i];", 2L),
                ("[1, 2, 3][3]", null),
                ("[1, 2, 3][-1]", null),
            };

            foreach (var (input, expected) in tests)
            {
                var evaluated = TestEval(input);
                if (expected is long l)
                    TestIntegerObject(evaluated, l);
                else
                    TestNullObject(evaluated);
            }
        }

        [Fact]
        public void TestHashLiteralss()
        {
            var input = @"let two = ""two"";
            {
                ""one"": 10 - 9,
                two: 1 + 1,
                ""thr"" + ""ee"": 6 / 2,
                4: 4,
                true: 5,
                false: 6
            }";

            var evaluated = TestEval(input);
            Assert.IsType<Hash>(evaluated);
            var result = (Hash)evaluated;

            var expected = new[]
            {
                (new String { Value = "one" }.HashKey(), 1L),
                (new String { Value = "two" }.HashKey(), 2L),
                (new String { Value = "three" }.HashKey(), 3L),
                (new Integer { Value = 4L }.HashKey(), 4L),
                (Evaluator.TRUE.HashKey(), 5L),
                (Evaluator.FALSE.HashKey(), 6L),
            };

            Assert.Equal(expected.Length, result.Pairs.Count);

            foreach (var (expectedKey, expectedValue) in expected)
            {
                Assert.True(result.Pairs.TryGetValue(expectedKey, out var pair));

                TestIntegerObject(pair.Value, expectedValue);
            }
        }

        [Fact]
        public void TestHashIndexExpressions()
        {
            var tests = new (string, object)[]
            {
                (@"{""foo"": 5}[""foo""]", 5L),
                (@"{""foo"": 5}[""bar""]", null),
                (@"let key = ""foo""; {""foo"": 5}[key]", 5L),
                (@"{}[""foo""]", null),
                ("{5: 5}[5]", 5L),
                ("{true: 5}[true]", 5L),
                ("{false: 5}[false]", 5L),
            };

            foreach (var (input, expected) in tests)
            {
                var evaluated = TestEval(input);
                if (expected is long l)
                    TestIntegerObject(evaluated, l);
                else
                    TestNullObject(evaluated);
            }
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
