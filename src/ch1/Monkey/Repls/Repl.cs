using System.IO;
using Monkey.Lexers;
using Monkey.Tokens;

namespace Monkey.Repls
{
    public class Repl
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

                for (var tok = l.NextToken(); tok.Type != TokenType.Eof; tok = l.NextToken())
                    @out.WriteLine(tok);
            }
        }
    }
}
