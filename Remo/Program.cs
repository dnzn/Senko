using System;
using static Remo.Konsole;
using static Remo.Global;

namespace Remo
{
    class Program
    {
        static void Main(string[] args)
        {
            Konsole Kon = new Konsole(ConsoleColor.Green);
            Konsole Log = new Konsole(true);

            string s1 = "hdmi to";
            string s2 = "Hdmi2";

            if (s1.IsSynonym(s2, true))
            {
                Kon.Write("Yes, {0} is synonymous with {1}.", s1.Encapsulate("["), s2.Encapsulate('['));
            }
            else
            {
                Kon.Write("No, {0} is not synonymous with {1}.", s1, s2);
            }

            Kon.ColorWriteLine(ConsoleColor.Yellow, Prefix.Indent, "Let's see if it works.");
            Kon.WriteLine(Prefix.Prompt, "This should be back to\nthe default color.");
            Log.WriteLine(Prefix.Indent, "This will only show up in the log.");

            Kon.WriteLine();

            Console.WriteLine(Konsole.Log);

            Console.ReadLine();
        }
    }
}
