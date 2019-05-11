namespace Senko
{
    using System;
    using Global;
    using System.Diagnostics;

    using static Kontext.Konsole;
    using static Kontext.Static;
    using static Global.Fields;

    class Program
    {
        static void Main(string[] args)
        {
            Welcome();

            //Kon.PromptColor = ConsoleColor.DarkGray;
            //Kon.PrimaryColor = ConsoleColor.Gray;

            Kon.WriteLine("<cyan>Hello World!\n<yellow>Let's test a newline.\nThis has no color tag.\n<green>Everything is working, so far.");
            
            Kon.Write("The quick brown fox jumps over the lazy dog.\nAnd everyone lives happily ever after.", new ColorSplit(SplitMethod.Char, Palette.RandomAuto));

            Kon.WriteLine("SPLIT\nTHIS\nINTO\n{0}.", PrefixType.Prompt, new ColorSplit(SplitMethod.Line, Palette.AutoMono), "LINES");

            Kon.WriteLine("A newline test...");

            Kon.Write("Test");

            Kon.Write("<random>Another test");

            //Kon.Color.ForceReset = false;
            Kon.Prefix.Current = PrefixType.Indent;

            Kon.Write("Last");

            Kon.Prefix.Current = PrefixType.Auto;

            Kon.WriteLine("This will be in the colors of the rainbow! Well, sort of...", new ColorSplit(SplitMethod.Word, Palette.RainbowWave));

            Kon.WriteLine();

            WriteLog();

            SonyDevice dev = new SonyDevice("hub");
            dev.Alias.Add("xbox", "action");
            
            Kon.WriteLine(dev.Info.Model);
            Kon.WriteLine(dev.Command.Code["Hdmi1"]);
            
            while (true)
            {
                //string read = Kon.ReadLine();

                //ParseCommand(read);                
            }
        }

        static void Welcome()
        {
            // 我が名は仙狐。
            // The project has been renamed to Senko, after the helpful fox in the anime "Sewayaki Kitsune no Senko-san"
            // Senko is also acronym for "SENd KOmmand".

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.ProductVersion;

            string[] asciiArray = new string[]
            {
                @"                      ,.",
                @"                 .*&@@@@# .#@@@%*",
                @"                #@@%,.&@&@@@//@%,",
                @"             .%@@(,  ,@@@&/  ,@#,",
                @"           ,#@%/      ,*.    .&@@(.",
                @"          *@@#                 *#@@@/.",
                @"         ,@&(                     ,/&@@%",
                @"        ,@%                 ,/%@@@&%/.",
                @"       *%@,               *@@@%*, ,/%&@@@@@@@@%#/.",
                @"       %@#               /&@,.*%&@@@%(*,...,,*#&@@%(",
                @"      ,@@,               .(@@@@%#,               *@@%",
                @"     ,#@%                                          *@%,",
                @"     ,%@%                                          ,@%,",
                @"      ,,.   ./%&@@@@@@@@@@@@@@@&%(,.             .%@@.  /*",
                @"         ,%@@@%(,.          ..*#%&@@@&%(,,,.,,*#&@@%.  ,@@,",
                @"      .#&@&*,                       ,(#&@@@@@@@&%*      &@(",
                @"    (@@%/                                               %@#",
                @"  *%@#*                                                 &@(",
                @" *@@/                                                  *@@,",
                @",@@&%#(,.                                             .@&(",
                @",##%%&@@@#,                                         /&@*",
                @"         /&@@(                                      (@@*",
                @"            ,%@@#.                               .%@@*",
                @"              ,/@@@/                           /#@@#",
                @"                 /%@@&(.                   ./#@@&/",
                @"                     .(&@@@@%#/*,,,,*(#%@@@@#/.",
                @"                         .,/%%&@@@@@&&%#*,." + Environment.NewLine
            };



            string ascii = asciiArray.Join(Environment.NewLine).ForceIndent(29);

            Kon.Prefix.Current = PrefixType.None;
            
            Kon.WriteLine(ascii, NewLineType.Both);
            Kon.WriteLine(new string('-', Console.WindowWidth) + Environment.NewLine);
            Kon.WriteLine("SENKO | Remote Control\n<gray>Version: {0}", ConsoleColor.White, version);
            Kon.WriteLine(@"https://github.com/TsurugiDanzen/Senko" + Environment.NewLine, ConsoleColor.Gray);

            Kon.Prefix.Current = PrefixType.Auto;

            Kon.Color.Reset();
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
