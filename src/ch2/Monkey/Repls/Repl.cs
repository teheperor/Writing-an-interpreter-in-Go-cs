using System.Collections.Generic;
using System.IO;
using Monkey.Lexers;
using Monkey.Parsers;

namespace Monkey.Repls
{
    public static class Repl
    {
        private const string PROMPT = ">>";

        public static void Start(TextReader @in, TextWriter @out)
        {
            while (true)
            {
                @out.Write(PROMPT);
                var line = @in.ReadLine();
                if (line == null)
                    return;

                var l = new Lexer(line);
                var p = new Parser(l);

                var program = p.ParseProgram();
                if (p.Errors.Count != 0)
                {
                    PrintParserErrors(@out, p.Errors);
                    continue;
                }

                @out.WriteLine(program.ToString());
            }
        }

        private const string MONKEY_FACE = @"            __,__
   .--.  .-""     ""-.  .--.
  / .. \/  .-. .-.  \/ .. \
 | |  '|  /   Y   \  |'  | |
 | \   \  \ 0 | 0 /  /   / |
  \ '- ,\.-""""""""""""""-./, -' /
   ''-' /_   ^ ^   _\ '-''
       |  \._ _./  |
       \   \ '~' /   /
        '._ '-=-' _.'
           '-----'
";

        private static void PrintParserErrors(TextWriter @out, IEnumerable<string> errors)
        {
            @out.Write(MONKEY_FACE);
            @out.WriteLine("Woops! We ran into some monkey business here!");
            @out.WriteLine(" parser errors:");
            foreach (var msg in errors)
                @out.WriteLine($"\t{msg}");
        }
    }
}
