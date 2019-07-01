using System;
using System.Collections.Generic;
using Monkey.Ast;
using Monkey.Tokens;
using Xunit;

namespace Monkey.Tests.Ast
{
    public class ModifyTest
    {
        [Fact]
        public void TestModify()
        {
            Func<IExpression> one = () => new IntegerLiteral { Value = 1L };
            Func<IExpression> two = () => new IntegerLiteral { Value = 2L };

            Func<INode, INode> turnOneIntoTwo = node =>
            {
                var integer = node as IntegerLiteral;
                if (integer == null)
                    return node;

                if (integer.Value != 1L)
                    return node;

                integer.Value = 2L;
                return integer;
            };

            var tests = new (INode, INode)[]
            {
                (one(),
                two()),
                (new Program {
                    Statements = new List<IStatement> {
                        new ExpressionStatement { Expression = one() }
                    },
                },
                new Program {
                    Statements = new List<IStatement> {
                        new ExpressionStatement { Expression = two() }
                    },
                }),
                (new InfixExpression { Left = one(), Operator = "+", Right = two() },
                new InfixExpression { Left = two(), Operator = "+", Right = two() }),
                (new InfixExpression { Left = two(), Operator = "+", Right = one() },
                new InfixExpression { Left = two(), Operator = "+", Right = two() }),
                (new PrefixExpression { Operator = "-", Right = one() },
                new PrefixExpression { Operator = "-", Right = two() }),
                (new IndexExpression { Left = one(), Index = one() },
                new IndexExpression { Left = two(), Index = two() }),
                (new IfExpression {
                    Condition = one(),
                    Consequence = new BlockStatement {
                        Statements = new List<IStatement> {
                            new ExpressionStatement { Expression = one() }
                        },
                    },
                    Alternative = new BlockStatement {
                        Statements = new List<IStatement> {
                            new ExpressionStatement { Expression = one() }
                        },
                    },
                },
                new IfExpression {
                    Condition = two(),
                    Consequence = new BlockStatement {
                        Statements = new List<IStatement> {
                            new ExpressionStatement { Expression = two() }
                        },
                    },
                    Alternative = new BlockStatement {
                        Statements = new List<IStatement> {
                            new ExpressionStatement { Expression = two() }
                        },
                    },
                }),
                (new ReturnStatement { ReturnValue = one(), Token = new Token { Literal = "return" } },
                new ReturnStatement { ReturnValue = two(), Token = new Token { Literal = "return" } }),
                (new LetStatement { Value = one(), Token = new Token { Literal = "let" }, Name = new Identifier { Value = "foo" } },
                new LetStatement { Value = two(), Token = new Token { Literal = "let" }, Name = new Identifier { Value = "foo" } }),
                (new FunctionLiteral {
                    Parameters = new List<Identifier> { },
                    Body = new BlockStatement {
                        Statements = new List<IStatement> {
                            new ExpressionStatement { Expression = one() }
                        },
                    },
                    Token = new Token { Literal = "fn" },
                },
                new FunctionLiteral {
                    Parameters = new List<Identifier> { },
                    Body = new BlockStatement {
                        Statements = new List<IStatement> {
                            new ExpressionStatement { Expression = two() }
                        },
                    },
                    Token = new Token { Literal = "fn" },
                }),
                (new ArrayLiteral { Elements = new List<IExpression> { one(), one() } },
                new ArrayLiteral { Elements = new List<IExpression> { two(), two() } }),
            };

            foreach (var (input, expected) in tests)
            {
                var modified = Monkey.Ast.Ast.Modify(input, turnOneIntoTwo);

                // @FIXME
                Assert.Equal(expected.ToString(), modified.ToString());
            }

            var hashLiteral = new HashLiteral {
                Pairs = new Dictionary<IExpression, IExpression> {
                    { one(), one() },
                    { one(), one() },
                }
            };

            //_.Modify(hashLiteral, turnOneIntoTwo);
            hashLiteral = (HashLiteral)Monkey.Ast.Ast.Modify(hashLiteral, turnOneIntoTwo);

            foreach (var (key, val) in hashLiteral.Pairs)
            {
                Assert.Equal(2, ((IntegerLiteral)key).Value);
                Assert.Equal(2, ((IntegerLiteral)val).Value);
            }
        }
    }
}
