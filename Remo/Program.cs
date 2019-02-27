using System;
using static Remo.Konsole;
using static Remo.Global;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

namespace Remo
{
    class Program
    {
        static void Main(string[] args)
        {
            Welcome();
            
            Kon.PromptColor = ConsoleColor.DarkGray;
            Kon.PrimaryColor = ConsoleColor.Gray;

            SonyDevice.Device dev = new SonyDevice.Device(@"C:\Users\Danzen Binos\OneDrive\remo\hub.deviceinfo.json");

            Kon.WriteLine(dev.Product);
            Kon.WriteLine(dev.Region);
            Kon.WriteLine(dev.Language);
            Kon.WriteLine(dev.Model);
            Kon.WriteLine(dev.Serial);
            Kon.WriteLine(dev.MacAddress);
            Kon.WriteLine(dev.Name);
            Kon.WriteLine(dev.Generation);
            Kon.WriteLine(dev.Area);
            Kon.WriteLine(dev.CID);

            while (true)
            {
                string read = Kon.ReadLine();

                ParseCommand(read);                
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
                Kon.Write(Prefix.Indent, Konsole.Log);
            }
            else if (command.IsSynonym("What are you?"))
            {
                Kon.Write(ConsoleColor.Yellow, Prefix.Indent, "I'm still a work in progress so there's nothing much to say.");
            }
            else if (command.IsSynonym("AddAlias"))
            {

            }
        }
    }
}
