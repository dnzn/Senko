using System;
using static Remo.Konsole;

namespace Remo
{
    class Program
    {
        static void Main(string[] args)
        {
            Konsole Kon = new Konsole(ConsoleColor.Green);


            Konsole.IgnoreColor = true;
            Kon.Write(NewLine.None, Prefix.Auto, "This text does not contain a newline. ");
            Kon.Color = ConsoleColor.Gray;
            Kon.WriteLine("So this must have no prompt.");
            Kon.WriteLine();
            Konsole.IgnoreColor = false;
            Kon.Color = ConsoleColor.Yellow;
            Kon.Write(NewLine.Below, Prefix.Indent, "Another test.");
            Console.ReadLine();
        }
    }
}
