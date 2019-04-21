namespace Remo
{
    using System;
    using static Remo.Konsole;
    using static Remo.Global;
    using System.Diagnostics;

    class Program
    {
        static void Main(string[] args)
        {
            Welcome();

            //Kon.PromptColor = ConsoleColor.DarkGray;
            //Kon.PrimaryColor = ConsoleColor.Gray;

            Konsole Kon = new Konsole();

            Kon.WriteLine("<cyan>Hello World!\n<yellow>Let's test a newline.\nThis has no color tag.\n<green>Everything is working, so far.");
            
            Kon.Write(Colors.PaletteWords("The quick brown fox jumps over the lazy dog.\n    And everyone lives happily ever after.", Colors.Palette.RandomLight));

            Kon.WriteLine("This <gray>should be <green>{0}.", Prefixes.Setting.Prompt, Colors.Palette.LightGradientWave, Colors.SplitMethod.Char, "INLINE");

            Kon.WriteLine("A newline test...");

            Kon.Write("Test");

            Kon.Write("<yellow>Another test");

            //Kon.Color.ForceReset = false;
            Kon.Prefix.Current = Prefixes.Setting.Indent;

            Kon.Write("Last");

            Kon.Prefix.Current = Prefixes.Setting.Auto;

            Kon.RainbowWrite("This will be in the colors of the rainbow! Well, sort of...");

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
