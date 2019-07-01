using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Monkey.Ast;
using Monkey.Lexers;
using Monkey.Parsers;
using Xunit;
using Xunit.Abstractions;


namespace Monkey.Tests.Parsers
{
    public class ParserTest
    {
        private readonly ITestOutputHelper output;

        public ParserTest(ITestOutputHelper output) => this.output = output;

        [Fact]
        public void TestLetStatements()
        {
            var tests = new (string, string, object)[] {
                ("let x = 5;", "x", 5L),
                ("let y = true;", "y", true),
                ("let foobar = y;", "foobar", "y"),
            };

            foreach (var (input, expectedIdentifier, expectedValue) in tests)
            {
                var l = new Lexer(input);
                var p = new Parser(l);
                var program = p.ParseProgram();
                CheckParserErrors(p);

                Assert.Equal(1, program.Statements.Count);

                var stmt = program.Statements[0];
                TestLetStatement(stmt, expectedIdentifier);

                var val = ((LetStatement)stmt).Value;
                TestLiteralExpression(val, expectedValue);
            }
        }

        [Fact]
        public void TestReturnStatements()
        {
            var tests = new (string, object)[] {
                ("return 5;", 5L),
                ("return true;", true),
                ("return foobar;", "foobar"),
            };

            foreach (var (input, expectedValue) in tests)
            {
                var l = new Lexer(input);
                var p = new Parser(l);
                var program = p.ParseProgram();
                CheckParserErrors(p);

                Assert.Equal(1, program.Statements.Count);

                var stmt = program.Statements[0];
                Assert.IsType<ReturnStatement>(stmt);
                var returnStmt = (ReturnStatement)stmt;
                Assert.Equal("return", returnStmt.TokenLiteral());
                TestLiteralExpression(returnStmt.ReturnValue, expectedValue);
            }
        }

        [Fact]
        public void TestIdentifierExpression()
        {
            var input = "foobar";

            var l = new Lexer(input);
            var p = new Parser(l);
            var program = p.ParseProgram();
            CheckParserErrors(p);

            Assert.Equal(1, program.Statements.Count);
            Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var stmt = (ExpressionStatement)program.Statements[0];

            Assert.IsType<Identifier>(stmt.Expression);
            var ident = (Identifier)stmt.Expression;
            Assert.Equal("foobar", ident.Value);
            Assert.Equal("foobar", ident.TokenLiteral());
        }

        [Fact]
        public void TestIntegerLiteralExpression()
        {
            var input = "5;";

            var l = new Lexer(input);
            var p = new Parser(l);
            var program = p.ParseProgram();
            CheckParserErrors(p);

            Assert.Equal(1, program.Statements.Count);
            Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var stmt = (ExpressionStatement)program.Statements[0];

            Assert.IsType<IntegerLiteral>(stmt.Expression);
            var literal = (IntegerLiteral)stmt.Expression;
            Assert.Equal(5, literal.Value);
            Assert.Equal("5", literal.TokenLiteral());
        }

        [Fact]
        public void TestParsingPrefixExpressions()
        {
            var prefixTests = new (string, string, object)[]
            {
                ("!5", "!", 5L),
                ("-15", "-", 15L),
                ("!true", "!", true),
                ("!false", "!", false),
            };

            foreach (var (input, @operator, value) in prefixTests)
            {
                var l = new Lexer(input);
                var p = new Parser(l);
                var program = p.ParseProgram();
                CheckParserErrors(p);

                Assert.Equal(1, program.Statements.Count);
                Assert.IsType<ExpressionStatement>(program.Statements[0]);
                var stmt = (ExpressionStatement)program.Statements[0];

                Assert.IsType<PrefixExpression>(stmt.Expression);
                var exp = (PrefixExpression)stmt.Expression;
                Assert.Equal(@operator, exp.Operator);
                TestLiteralExpression(exp.Right, value);
            }
        }

        [Fact]
        public void TestParsingInfixExpressions()
        {
            var infixTests = new (string, object, string, object)[]
            {
                ("5 + 5", 5L, "+", 5L),
                ("5 - 5", 5L, "-", 5L),
                ("5 * 5", 5L, "*", 5L),
                ("5 / 5", 5L, "/", 5L),
                ("5 > 5", 5L, ">", 5L),
                ("5 < 5", 5L, "<", 5L),
                ("5 == 5", 5L, "==", 5L),
                ("5 != 5", 5L, "!=", 5L),
                ("true == true", true, "==", true),
            };

            foreach (var (input, leftValue, @operator, rightValue) in infixTests)
            {
                var l = new Lexer(input);
                var p = new Parser(l);
                var program = p.ParseProgram();
                CheckParserErrors(p);

                Assert.Equal(1, program.Statements.Count);
                Assert.IsType<ExpressionStatement>(program.Statements[0]);
                var stmt = (ExpressionStatement)program.Statements[0];

                TestInfixExpression(stmt.Expression, leftValue, @operator, rightValue);
            }
        }

        [Fact]
        public void TestOperatorPrecedenceParsing()
        {
            var tests = new[]
            {
                ("-a * b", "((-a) * b)"),
                ("!-a", "(!(-a))"),
                ("a + b + c", "((a + b) + c)"),
                ("a + b - c", "((a + b) - c)"),
                ("a * b * c", "((a * b) * c)"),
                ("a * b / c", "((a * b) / c)"),
                ("a + b / c", "(a + (b / c))"),
                ("a + b * c + d / e - f", "(((a + (b * c)) + (d / e)) - f)"),
                ("3 + 4; -5 * 5", "(3 + 4)((-5) * 5)"),
                ("5 > 4 == 3 < 4", "((5 > 4) == (3 < 4))"),
                ("5 < 4 != 3 > 4", "((5 < 4) != (3 > 4))"),
                ("3 + 4 * 5 == 3 * 1 + 4 * 5", "((3 + (4 * 5)) == ((3 * 1) + (4 * 5)))"),
                ("true", "true"),
                ("false", "false"),
                ("3 > 5 == false", "((3 > 5) == false)"),
                ("3 < 5 == true", "((3 < 5) == true)"),
                ("1 + (2 + 3) + 4", "((1 + (2 + 3)) + 4)"),
                ("(5 + 5) * 2", "((5 + 5) * 2)"),
                ("2 / (5 + 5)", "(2 / (5 + 5))"),
                ("-(5 + 5)", "(-(5 + 5))"),
                ("!(true == true)", "(!(true == true))"),
                ("a + add(b * c) + d", "((a + add((b * c))) + d)"),
                ("add(a, b, 1, 2 * 3, 4 + 5, add(6, 7 * 8))", "add(a, b, 1, (2 * 3), (4 + 5), add(6, (7 * 8)))"),
                ("add(a + b + c * d / f + g)", "add((((a + b) + ((c * d) / f)) + g))"),
                ("a * [1, 2, 3, 4][b * c] * d", "((a * ([1, 2, 3, 4][(b * c)])) * d)"),
                ("add(a * b[2], b[1], 2 * [1, 2][1])", "add((a * (b[2])), (b[1]), (2 * ([1, 2][1])))"),
            };

            foreach (var (input, expected) in tests)
            {
                var l = new Lexer(input);
                var p = new Parser(l);
                var program = p.ParseProgram();
                CheckParserErrors(p);

                var actual = program.ToString();
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void TestBooleanExpression()
        {
            var tests = new[]
            {
                ("true;", true),
                ("false;", false),
            };

            foreach (var (input, expectedBoolean) in tests)
            {
                var l = new Lexer(input);
                var p = new Parser(l);
                var program = p.ParseProgram();
                CheckParserErrors(p);

                Assert.Equal(1, program.Statements.Count);

                Assert.IsType<ExpressionStatement>(program.Statements[0]);
                var stmt = (ExpressionStatement)program.Statements[0];

                Assert.IsType<Monkey.Ast.Boolean>(stmt.Expression);
                var boolean = (Monkey.Ast.Boolean)stmt.Expression;
                Assert.Equal(expectedBoolean, boolean.Value);
            }
        }

        [Fact]
        public void TestIfExpression()
        {
            var input = "if (x < y) { x }";

            var l = new Lexer(input);
            var p = new Parser(l);
            var program = p.ParseProgram();
            CheckParserErrors(p);

            Assert.Equal(1, program.Statements.Count);

            Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var stmt = (ExpressionStatement)program.Statements[0];

            Assert.IsType<IfExpression>(stmt.Expression);
            var exp = (IfExpression)stmt.Expression;

            TestInfixExpression(exp.Condition, "x", "<", "y");

            Assert.Equal(1, exp.Consequence.Statements.Count);

            Assert.IsType<ExpressionStatement>(exp.Consequence.Statements[0]);
            var consequence = (ExpressionStatement)exp.Consequence.Statements[0];

            TestIdentifier(consequence.Expression, "x");

            Assert.Null(exp.Alternative);
        }

        [Fact]
        public void TestIfElseExpression()
        {
            var input = "if (x < y) { x } else { y }";

            var l = new Lexer(input);
            var p = new Parser(l);
            var program = p.ParseProgram();
            CheckParserErrors(p);

            Assert.Equal(1, program.Statements.Count);

            Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var stmt = (ExpressionStatement)program.Statements[0];

            Assert.IsType<IfExpression>(stmt.Expression);
            var exp = (IfExpression)stmt.Expression;

            TestInfixExpression(exp.Condition, "x", "<", "y");

            Assert.Equal(1, exp.Consequence.Statements.Count);

            Assert.IsType<ExpressionStatement>(exp.Consequence.Statements[0]);
            var consequence = (ExpressionStatement)exp.Consequence.Statements[0];

            TestIdentifier(consequence.Expression, "x");

            Assert.Equal(1, exp.Alternative.Statements.Count);

            Assert.IsType<ExpressionStatement>(exp.Alternative.Statements[0]);
            var alternative = (ExpressionStatement)exp.Alternative.Statements[0];

            TestIdentifier(alternative.Expression, "y");
        }

        [Fact]
        public void TestFunctionLiteralParsing()
        {
            var input = "fn(x, y) { x + y; }";

            var l = new Lexer(input);
            var p = new Parser(l);
            var program = p.ParseProgram();
            CheckParserErrors(p);

            Assert.Equal(1, program.Statements.Count);

            Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var stmt = (ExpressionStatement)program.Statements[0];

            Assert.IsType<FunctionLiteral>(stmt.Expression);
            var function = (FunctionLiteral)stmt.Expression;

            Assert.Equal(2, function.Parameters.Count);

            TestLiteralExpression(function.Parameters[0], "x");
            TestLiteralExpression(function.Parameters[1], "y");

            Assert.Equal(1, function.Body.Statements.Count);

            Assert.IsType<ExpressionStatement>(function.Body.Statements[0]);
            var bodyStmt = (ExpressionStatement)function.Body.Statements[0];

            TestInfixExpression(bodyStmt.Expression, "x", "+", "y");
        }

        [Fact]
        public void TestFunctionParametersParsing()
        {
            var tests = new[]
            {
                ("fn() {}", new string[]{ }),
                ("fn(x) {}", new[]{ "x" }),
                ("fn(x, y, z) {}", new[]{ "x", "y", "z" }),
            };

            foreach (var (input, expectedParams) in tests)
            {
                var l = new Lexer(input);
                var p = new Parser(l);
                var program = p.ParseProgram();
                CheckParserErrors(p);

                var stmt = program.Statements[0] as ExpressionStatement;
                var function = stmt.Expression as FunctionLiteral;

                Assert.Equal(expectedParams.Length, function.Parameters.Count);

                foreach (var (ident, param) in expectedParams.Zip(function.Parameters, (i, _p) => (i, _p)))
                    TestLiteralExpression(param, ident);
            }
        }

        [Fact]
        public void TestCallExpressionParsing()
        {
            var input = "add(1, 2 * 3, 4 + 5)";

            var l = new Lexer(input);
            var p = new Parser(l);
            var program = p.ParseProgram();
            CheckParserErrors(p);

            Assert.Equal(1, program.Statements.Count);

            Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var stmt = (ExpressionStatement)program.Statements[0];

            Assert.IsType<CallExpression>(stmt.Expression);
            var exp = (CallExpression)stmt.Expression;

            TestIdentifier(exp.Function, "add");

            Assert.Equal(3, exp.Arguments.Count);

            TestLiteralExpression(exp.Arguments[0], 1L);
            TestInfixExpression(exp.Arguments[1], 2L, "*", 3L);
            TestInfixExpression(exp.Arguments[2], 4L, "+", 5L);
        }

        [Fact]
        public void TestStringLiteralExpression()
        {
            var input = @"""hello world""";

            var l = new Lexer(input);
            var p = new Parser(l);
            var program = p.ParseProgram();
            CheckParserErrors(p);

            var stmt = (ExpressionStatement)program.Statements[0];
            Assert.IsType<StringLiteral>(stmt.Expression);
            var literal = (StringLiteral)stmt.Expression;

            Assert.Equal("hello world", literal.Value);
        }

        [Fact]
        public void TestParsingArrayLiterals()
        {
            var input = "[1, 2 * 2, 3 + 3]";

            var l = new Lexer(input);
            var p = new Parser(l);
            var program = p.ParseProgram();
            CheckParserErrors(p);

            var stmt = (ExpressionStatement)program.Statements[0];
            Assert.IsType<ArrayLiteral>(stmt.Expression);
            var array = (ArrayLiteral)stmt.Expression;

            Assert.Equal(3, array.Elements.Count);

            TestIntegerLiteral(array.Elements[0], 1L);
            TestInfixExpression(array.Elements[1], 2L, "*", 2L);
            TestInfixExpression(array.Elements[2], 3L, "+", 3L);
        }

        [Fact]
        public void TestParsingIndexExpressions()
        {
            var input = "myArray[1 + 1]";

            var l = new Lexer(input);
            var p = new Parser(l);
            var program = p.ParseProgram();
            CheckParserErrors(p);

            var stmt = (ExpressionStatement)program.Statements[0];
            Assert.IsType<IndexExpression>(stmt.Expression);
            var indexExp = (IndexExpression)stmt.Expression;

            TestIdentifier(indexExp.Left, "myArray");
            TestInfixExpression(indexExp.Index, 1L, "+", 1L);
        }

        [Fact]
        public void TestParsingEmptyHashLiteral()
        {
            var input = "{}";

            var l = new Lexer(input);
            var p = new Parser(l);
            var program = p.ParseProgram();
            CheckParserErrors(p);

            var stmt = (ExpressionStatement)program.Statements[0];
            Assert.IsType<HashLiteral>(stmt.Expression);
            var hash = (HashLiteral)stmt.Expression;

            Assert.Equal(0, hash.Pairs.Count);
        }

        [Fact]
        public void TestParsingHashLiteralsStringKeys()
        {
            var input = @"{""one"": 1, ""two"": 2, ""three"": 3}";

            var l = new Lexer(input);
            var p = new Parser(l);
            var program = p.ParseProgram();
            CheckParserErrors(p);

            var stmt = (ExpressionStatement)program.Statements[0];
            Assert.IsType<HashLiteral>(stmt.Expression);
            var hash = (HashLiteral)stmt.Expression;

            Assert.Equal(3, hash.Pairs.Count);

            var expected = new Dictionary<string, long>
            {
                { "one", 1L },
                { "two", 2L },
                { "three", 3L },
            }.ToImmutableDictionary();

            foreach (var (key, value) in hash.Pairs)
            {
                Assert.IsType<StringLiteral>(key);
                var literal = (StringLiteral)key;

                var expectedValue = expected.GetValueOrDefault(literal.ToString());

                TestIntegerLiteral(value, expectedValue);
            }
        }

        [Fact]
        public void TestParsingHashLiteralsWithExpressions()
        {
            var input = @"{""one"": 0 + 1, ""two"": 10 - 8, ""three"": 15 / 5}";

            var l = new Lexer(input);
            var p = new Parser(l);
            var program = p.ParseProgram();
            CheckParserErrors(p);

            var stmt = (ExpressionStatement)program.Statements[0];
            Assert.IsType<HashLiteral>(stmt.Expression);
            var hash = (HashLiteral)stmt.Expression;

            Assert.Equal(3, hash.Pairs.Count);

            var expected = new Dictionary<string, Action<IExpression>>
            {
                { "one", e => TestInfixExpression(e, 0L, "+", 1L) },
                { "two", e => TestInfixExpression(e, 10L, "-", 8L) },
                { "three", e => TestInfixExpression(e, 15L, "/", 5L) },
            }.ToImmutableDictionary();

            foreach (var (key, value) in hash.Pairs)
            {
                Assert.IsType<StringLiteral>(key);
                var literal = (StringLiteral)key;

                var testFunc = expected.GetValueOrDefault(literal.ToString());
                Assert.NotNull(testFunc);

                testFunc(value);
            }
        }

        private void TestLetStatement(IStatement s, string name)
        {
            Assert.Equal("let", s.TokenLiteral());

            Assert.IsType<LetStatement>(s);
            var letStmt = (LetStatement)s;

            Assert.Equal(name, letStmt.Name.Value);

            Assert.Equal(name, letStmt.Name.TokenLiteral());
        }

        private void TestInfixExpression(IExpression exp, object left, string @operator, object right)
        {
            Assert.IsType<InfixExpression>(exp);
            var opExp = (InfixExpression)exp;

            TestLiteralExpression(opExp.Left, left);

            Assert.Equal(@operator, opExp.Operator);

            TestLiteralExpression(opExp.Right, right);
        }

        private void TestLiteralExpression(IExpression exp, object expected)
        {
            switch (expected)
            {
                case long v: TestIntegerLiteral(exp, v); break;
                case string v: TestIdentifier(exp, v); break;
                case bool v: TestBooleanLiteral(exp, v); break;
                default: Assert.True(false, $"type of exp not handed. got={exp.GetType().Name}"); break;
            }
        }

        private void TestIntegerLiteral(IExpression il, long value)
        {
            Assert.IsType<IntegerLiteral>(il);
            var integ = (IntegerLiteral)il;

            Assert.Equal(value, integ.Value);

            Assert.Equal(value.ToString(), integ.TokenLiteral());
        }

        private void TestIdentifier(IExpression exp, string value)
        {
            Assert.IsType<Identifier>(exp);
            var ident = (Identifier)exp;

            Assert.Equal(value, ident.Value);

            Assert.Equal(value, ident.TokenLiteral());
        }

        private void TestBooleanLiteral(IExpression exp, bool value)
        {
            Assert.IsType<Monkey.Ast.Boolean>(exp);
            var bo = (Monkey.Ast.Boolean)exp;

            Assert.Equal(value, bo.Value);

            Assert.Equal(value.ToString().ToLower(), bo.TokenLiteral());
        }

        private void CheckParserErrors(Parser p)
        {
            var errors = p.Errors;
            if (errors.Count == 0)
                return;

            output.WriteLine($"parser has {errors.Count} errors");
            foreach (var msg in errors)
                output.WriteLine($"parser error: {msg}");
        }
    }
}
