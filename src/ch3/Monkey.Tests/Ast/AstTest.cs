using System.Collections.Generic;
using Monkey.Ast;
using Monkey.Tokens;
using Xunit;

namespace Monkey.Tests.Ast
{
    public class AstTest
    {
        [Fact]
        public void TestString()
        {
            var program = new Program
            {
                Statements = new List<IStatement> {
                    new LetStatement {
                        Token = new Token { Type = TokenType.Let, Literal = "let" },
                        Name = new Identifier {
                            Token = new Token { Type = TokenType.Ident, Literal = "myVar" },
                            Value = "myVar",
                        },
                        Value = new Identifier {
                            Token = new Token { Type = TokenType.Ident, Literal = "anotherVar" },
                            Value = "anotherVar",
                        },
                    },
                },
            };

            Assert.Equal("let myVar = anotherVar;", program.ToString());
        }
    }
}
