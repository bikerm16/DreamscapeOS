using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DreamscapeCore;

namespace DreamscapeTextShell
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.SetWindowSize(80, 25);
            Console.SetBufferSize(Console.WindowWidth, Console.WindowHeight);
            
            Console.Write(Interpreter.Init(true));

            Executable hello = new Executable("hello", "/home", Method, true);
            Interpreter.AddProgram(hello);

            while (true)
            {
                Console.Write(Interpreter.Parse(Console.ReadLine()));
            }
        }

        private static void Method(string[] args)
        {
            Console.WriteLine("Hello");
        }
    }
}
