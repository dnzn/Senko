using System;
using static Remo.Konsole;
using static Remo.Global;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Text.RegularExpressions;

namespace Remo
{
    class Program
    {
        static void Main(string[] args)
        {
            Welcome();

            //Kon.PromptColor = ConsoleColor.DarkGray;
            //Kon.PrimaryColor = ConsoleColor.Gray;

            Konsole Kon = new Konsole();

            Console.WriteLine("Test message...");

            Console.WriteLine(Kon.Color.Paint(@"<cyan>This is a test"));

            foreach (string s in Kon.Color.Split(@"This is a test. <\red>This should not be in a new line. <blue>However, this one will be."))
            {
                Kon.Color.Toggle(Kon.Color.Random);
                Console.WriteLine(s);
            }

            SonyDevice dev = new SonyDevice("hub");
            dev.Alias.Add("xbox", "action");
            
            //Kon.WriteLine(Prefix.Prompt, dev.Info.Model);
            
            while (true)
            {
                //string read = Kon.ReadLine();

                //ParseCommand(read);                
            }
        }

        static void Welcome()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            Console.ForegroundColor = ConsoleColor.White;

            Console.Write("REMO [Konohana] ");
            Console.WriteLine("version: " + version);
            Console.WriteLine(@"https://github.com/TsurugiDanzen/Remo" + Environment.NewLine);

            Console.ResetColor();
        }

        static void ParseCommand(string command)
        {
            if (command.IsSynonym("show konsole.log"))
            {
                //Kon.Write(Prefix.Indent, Konsole.Log);
            }
            else if (command.IsSynonym("What are you?"))
            {
                //Kon.Write(ConsoleColor.Yellow, Prefix.Indent, "I'm still a work in progress so there's nothing much to say.");
            }
            else if (command.IsSynonym("AddAlias"))
            {

            }
        }
    }
}
