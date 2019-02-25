using System;
using static Remo.Konsole;
using static Remo.Global;
using System.Diagnostics;

namespace Remo
{
    class Program
    {
        static void Main(string[] args)
        {
            Welcome();

            Konsole Kon = new Konsole();
            Konsole Log = new Konsole();

            Kon.PrimaryColor = ConsoleColor.Cyan;
            Kon.PromptColor = ConsoleColor.DarkGray;
            
            Kon.Write(NewLine.After, "Testing {0}, {1}, {2}...", "one", "two", "three");
            Kon.Write(NewLine.After, Prefix.Auto, "Testing {0}, {1}, {2}...", 4, 5, 6);
            Kon.Write(ConsoleColor.Green, NewLine.None, Prefix.Indent, "Testing NewLine.None with Prefix.Indent.");
            Kon.Write(NewLine.None, Prefix.Indent, "This is an inline write.");
            Kon.Write(NewLine.After, Prefix.Prompt, "This forces a prompt so it jumps to the next line.");
            Kon.Write(NewLine.After, Prefix.Prompt, "Let's make a long string with line breaks.\nThe quick brown fox jumps over the lazy dog.");
            Kon.Write(NewLine.Both, Prefix.None, "Let's try Prefix.None with NewLine.Both");

            Kon.WriteLine("Trying WriteLine() with no additional parameters.");
            Kon.WriteLine(NewLine.Both, Prefix.Prompt, "Let's set it to Prefix.Prompt.");

            Console.WriteLine();

            Kon.Write(ConsoleColor.Gray, Prefix.Indent, Kon.Log);

            Console.ReadLine();
        }

        static void Welcome()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("Welcome to Remo");
            Console.WriteLine("Version: " + version);
            Console.WriteLine("Still a work in progress." + Environment.NewLine);

            Console.ResetColor();
        }
    }
}
