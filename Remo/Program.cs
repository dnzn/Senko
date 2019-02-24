using System;
using static Remo.Konsole;

namespace Remo
{
    class Program
    {
        static void Main(string[] args)
        {
            Konsole Kon = new Konsole(ConsoleColor.Green);

            Kon.Write(NewLine.None, Prefix.Auto, "This text does not contain a newline. ");
            Kon.Color = ConsoleColor.Gray;
            Kon.WriteLine("So this must have no prompt.");
            Kon.WriteLine();
            Kon.Color = ConsoleColor.Yellow;
            Kon.WriteLine(Prefix.Indent, "Another test.");
            Console.ReadLine();
        }
    }
}
