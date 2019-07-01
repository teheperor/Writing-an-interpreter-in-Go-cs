using System;
using Monkey.Repls;

namespace Monkey
{
    class Monkey
    {
        static void Main(string[] args)
        {
            var user = Environment.UserName;
            Console.WriteLine($"Hello {user}! This is the Monkey programming language!");
            Console.WriteLine("Feel free to type in commands");
            Repl.Start(Console.In, Console.Out);
        }
    }
}
