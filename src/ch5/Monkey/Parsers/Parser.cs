using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Monkey.Ast;
using Monkey.Lexers;
using Monkey.Tokens;

namespace Monkey.Parsers
{
    using InfixParseFn = Func<IExpression, IExpression>;
    using PrefixParseFn = Func<IExpression>;

    public class Parser
    {
        private enum Precedence
        {
            Lowest,
            Equals,  // ==
            LessGreater,  // > or <
            Sum,  // +
            Product,  // *
            Prefix,  // -X or !X
            Call,  // myFunction(X)
            Index,  // array[index]
        };

        private static readonly IImmutableDictionary<TokenType, Precedence> precedences = new Dictionary<TokenType, Precedence>
        {
            { TokenType.Eq, Precedence.Equals },
            { TokenType.NotEq, Precedence.Equals },
            { TokenType.LT, Precedence.LessGreater },
            { TokenType.GT, Precedence.LessGreater },
            { TokenType.Plus, Precedence.Sum },
            { TokenType.Minus, Precedence.Sum },
            { TokenType.Slash, Precedence.Product },
            { TokenType.Asterisk, Precedence.Product },
            { TokenType.LParen, Precedence.Call },
            { TokenType.LBracket, Precedence.Index },
        }.ToImmutableDictionary();

        private readonly Lexer l;

        private Token curToken;
        private Token peekToken;

        private readonly IImmutableDictionary<TokenType, PrefixParseFn> prefixParseFns;
        private readonly IImmutableDictionary<TokenType, InfixParseFn> infixParseFns;

        public IList<string> Errors { get; set; }

        public Parser(Lexer l)
        {
            this.l = l;
            Errors = new List<string>();

            prefixParseFns = new Dictionary<TokenType, PrefixParseFn>
            {
                { TokenType.Ident, () => ParseIdentifier() },
                { TokenType.Int, () => ParseIntegerLiteral() },
                { TokenType.String, () => ParseStringLiteral() },
                { TokenType.Bang, () => ParsePrefixExpression() },
                { TokenType.Minus, () => ParsePrefixExpression() },
                { TokenType.True, () => ParseBoolean() },
                { TokenType.False , () => ParseBoolean() },
                { TokenType.LParen, () => ParseGroupedExpression() },
                { TokenType.If, () => ParseIfExpression() },
                { TokenType.Function, () => ParseFunctionLiteral() },
                { TokenType.LBracket, () => ParseArrayLiteral() },
                { TokenType.LBrace, () => ParseHashLiteral() },
                { TokenType.Macro, () => ParseMacroLiteral() },
            }.ToImmutableDictionary();

            infixParseFns = new Dictionary<TokenType, InfixParseFn>
            {
                { TokenType.Plus, x => ParseInfixExpression(x) },
                { TokenType.Minus, x => ParseInfixExpression(x) },
                { TokenType.Slash, x => ParseInfixExpression(x) },
                { TokenType.Asterisk, x => ParseInfixExpression(x) },
                { TokenType.Eq, x => ParseInfixExpression(x) },
                { TokenType.NotEq, x => ParseInfixExpression(x) },
                { TokenType.LT, x => ParseInfixExpression(x) },
                { TokenType.GT, x => ParseInfixExpression(x) },
                { TokenType.LParen, x => ParseCallExpression(x) },
                { TokenType.LBracket, x => ParseIndexExpression(x) },
            }.ToImmutableDictionary();

            NextToken();
            NextToken();
        }

        public void NextToken()
        {
            curToken = peekToken;
            peekToken = l.NextToken();
        }

        private bool CurTokenIs(TokenType t) => curToken.Type == t;

        private bool PeekTokenIs(TokenType t) => peekToken.Type == t;

        private bool ExpectPeek(TokenType t)
        {
            if (PeekTokenIs(t))
            {
                NextToken();
                return true;
            }
            else
            {
                PeekError(t);
                return false;
            }
        }

        public void PeekError(TokenType t)
        {
            var msg = $"expected next token to be {t}, got {peekToken.Type} instead";
            Errors.Add(msg);
        }

        private void NoPrefixParserFnError(TokenType t)
        {
            var msg = $"no prefix parse function for {t} found";
            Errors.Add(msg);
        }

        public Program ParseProgram()
        {
            var program = new Program { Statements = new List<IStatement>() };

            while (curToken.Type != TokenType.Eof)
            {
                var stmt = ParseStatement();
                if (stmt != null)
                    program.Statements.Add(stmt);
                NextToken();
            }

            return program;
        }

        private IStatement ParseStatement()
        {
            switch (curToken.Type)
            {
                case TokenType.Let: return ParseLetStatement();
                case TokenType.Return: return ParseReturnStatement();
                default: return ParseExpressionStatement();
            }
        }

        private LetStatement ParseLetStatement()
        {
            var stmt = new LetStatement { Token = curToken };

            if (!ExpectPeek(TokenType.Ident))
                return null;

            stmt.Name = new Identifier { Token = curToken, Value = curToken.Literal };

            if (!ExpectPeek(TokenType.Assign))
                return null;

            NextToken();

            stmt.Value = ParseExpression(Precedence.Lowest);

            if (PeekTokenIs(TokenType.Semicolon))
                NextToken();

            return stmt;
        }

        private ReturnStatement ParseReturnStatement()
        {
            var stmt = new ReturnStatement { Token = curToken };

            NextToken();

            stmt.ReturnValue = ParseExpression(Precedence.Lowest);

            while (!CurTokenIs(TokenType.Semicolon))
                NextToken();

            return stmt;
        }

        private ExpressionStatement ParseExpressionStatement()
        {
            var stmt = new ExpressionStatement { Token = curToken };

            stmt.Expression = ParseExpression(Precedence.Lowest);

            if (PeekTokenIs(TokenType.Semicolon))
                NextToken();

            return stmt;
        }

        private IExpression ParseExpression(Precedence precedence)
        {
            if (!prefixParseFns.TryGetValue(curToken.Type, out var prefix))
            {
                NoPrefixParserFnError(curToken.Type);
                return null;
            }
            var leftExp = prefix();

            while (!PeekTokenIs(TokenType.Semicolon) && precedence < PeekPrecedence())
            {
                if (!infixParseFns.TryGetValue(peekToken.Type, out var infix))
                    return leftExp;

                NextToken();

                leftExp = infix(leftExp);
            }

            return leftExp;
        }

        private Precedence PeekPrecedence() =>
            precedences.TryGetValue(peekToken.Type, out var p) ? p : Precedence.Lowest;

        private Precedence CurPrecedence() =>
            precedences.TryGetValue(curToken.Type, out var p) ? p : Precedence.Lowest;

        private IExpression ParseIdentifier() =>
            new Identifier { Token = curToken, Value = curToken.Literal };

        private IExpression ParseIntegerLiteral()
        {
            var lit = new IntegerLiteral { Token = curToken };

            (long result, bool success) Parse(string s)
            {
                try
                {
                    long result;
                    if (s.StartsWith("0x")) result = Convert.ToInt64(s, 16);
                    else if (s.StartsWith("0")) result = Convert.ToInt64(s, 8);
                    else result = Convert.ToInt64(s);
                    return (result, true);
                }
                catch
                {
                    return (0, false);
                }
            }

            var (value, success) = Parse(curToken.Literal);
            if (!success)
            {
                var msg = $@"could not parse ""{curToken.Literal}"" as integer";
                Errors.Add(msg);
                return null;
            }

            lit.Value = value;

            return lit;
        }

        private IExpression ParseStringLiteral() =>
            new StringLiteral { Token = curToken, Value = curToken.Literal };

        private IExpression ParsePrefixExpression()
        {
            var expression = new PrefixExpression { Token = curToken, Operator = curToken.Literal };

            NextToken();

            expression.Right = ParseExpression(Precedence.Prefix);

            return expression;
        }

        private IExpression ParseInfixExpression(IExpression left)
        {
            var expression = new InfixExpression { Token = curToken, Operator = curToken.Literal, Left = left };

            var precedence = CurPrecedence();
            NextToken();
            expression.Right = ParseExpression(precedence);

            return expression;
        }

        private IExpression ParseBoolean() =>
            new Ast.Boolean { Token = curToken, Value = CurTokenIs(TokenType.True) };

        private IExpression ParseGroupedExpression()
        {
            NextToken();

            var exp = ParseExpression(Precedence.Lowest);
            if (!ExpectPeek(TokenType.RParen))
                return null;

            return exp;
        }

        private IExpression ParseIfExpression()
        {
            var expression = new IfExpression { Token = curToken };

            if (!ExpectPeek(TokenType.LParen))
                return null;

            NextToken();
            expression.Condition = ParseExpression(Precedence.Lowest);

            if (!ExpectPeek(TokenType.RParen))
                return null;

            if (!ExpectPeek(TokenType.LBrace))
                return null;

            expression.Consequence = ParseBlockStatement();

            if (PeekTokenIs(TokenType.Else))
            {
                NextToken();

                if (!ExpectPeek(TokenType.LBrace))
                    return null;

                expression.Alternative = ParseBlockStatement();
            }

            return expression;
        }

        private BlockStatement ParseBlockStatement()
        {
            var block = new BlockStatement { Token = curToken, Statements = new List<IStatement>() };

            NextToken();

            while (!CurTokenIs(TokenType.RBrace) && !CurTokenIs(TokenType.Eof))
            {
                var stmt = ParseStatement();
                if (stmt != null)
                    block.Statements.Add(stmt);
                NextToken();
            }

            return block;
        }

        private FunctionLiteral ParseFunctionLiteral()
        {
            var lit = new FunctionLiteral { Token = curToken };

            if (!ExpectPeek(TokenType.LParen))
                return null;

            lit.Parameters = ParseFunctionParameters();

            if (!ExpectPeek(TokenType.LBrace))
                return null;

            lit.Body = ParseBlockStatement();

            return lit;
        }

        private IList<Identifier> ParseFunctionParameters()
        {
            var identifiers = new List<Identifier>();

            if (PeekTokenIs(TokenType.RParen))
            {
                NextToken();
                return identifiers;
            }

            NextToken();

            var ident = new Identifier { Token = curToken, Value = curToken.Literal };
            identifiers.Add(ident);

            while (PeekTokenIs(TokenType.Comma))
            {
                NextToken();
                NextToken();
                ident = new Identifier { Token = curToken, Value = curToken.Literal };
                identifiers.Add(ident);
            }

            if (!ExpectPeek(TokenType.RParen))
                return null;

            return identifiers;
        }

        private IExpression ParseCallExpression(IExpression function)
        {
            var exp = new CallExpression { Token = curToken, Function = function };
            exp.Arguments = ParseExpressionList(TokenType.RParen);
            return exp;
        }

        private IList<IExpression> ParseExpressionList(TokenType end)
        {
            var list = new List<IExpression>();

            if (PeekTokenIs(end))
            {
                NextToken();
                return list;
            }

            NextToken();
            list.Add(ParseExpression(Precedence.Lowest));

            while (PeekTokenIs(TokenType.Comma))
            {
                NextToken();
                NextToken();
                list.Add(ParseExpression(Precedence.Lowest));
            }

            if (!ExpectPeek(end))
                return null;

            return list;
        }

        private IExpression ParseArrayLiteral()
        {
            var array = new ArrayLiteral { Token = curToken };

            array.Elements = ParseExpressionList(TokenType.RBracket);

            return array;
        }

        private IExpression ParseIndexExpression(IExpression left)
        {
            var exp = new IndexExpression { Token = curToken, Left = left };

            NextToken();
            exp.Index = ParseExpression(Precedence.Lowest);

            if (!ExpectPeek(TokenType.RBracket))
                return null;

            return exp;
        }

        private IExpression ParseHashLiteral()
        {
            var hash = new HashLiteral { Token = curToken, Pairs = new Dictionary<IExpression, IExpression>() };

            while (!PeekTokenIs(TokenType.RBrace))
            {
                NextToken();
                var key = ParseExpression(Precedence.Lowest);

                if (!ExpectPeek(TokenType.Colon))
                    return null;

                NextToken();
                var value = ParseExpression(Precedence.Lowest);

                hash.Pairs[key] = value;

                if (!PeekTokenIs(TokenType.RBrace) && !ExpectPeek(TokenType.Comma))
                    return null;
            }

            if (!ExpectPeek(TokenType.RBrace))
                return null;

            return hash;
        }

        private IExpression ParseMacroLiteral()
        {
            var lit = new MacroLiteral { Token = curToken };

            if (!ExpectPeek(TokenType.LParen))
                return null;

            lit.Parameters = ParseFunctionParameters();

            if (!ExpectPeek(TokenType.LBrace))
                return null;

            lit.Body = ParseBlockStatement();

            return lit;
        }
    }
}
