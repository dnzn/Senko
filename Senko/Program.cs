namespace Senko
{
    using System;
    using Global;
    using System.Diagnostics;

    using static Konsole.Kontext;
    using static Konsole.Static;
    using static Konsole.Kontext.Colors;
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

            Kon.WriteLine("SPLIT\nTHIS\nINTO\n{0}.", Prefixes.Setting.Prompt, new ColorSplit(SplitMethod.Line, Palette.AutoMono), "LINES");

            Kon.WriteLine("A newline test...");

            Kon.Write("Test");

            Kon.Write("<random>Another test");

            //Kon.Color.ForceReset = false;
            Kon.Prefix.Current = Prefixes.Setting.Indent;

            Kon.Write("Last");

            Kon.Prefix.Current = Prefixes.Setting.Auto;

            Kon.Write("This will be in the colors of the rainbow! Well, sort of...", new ColorSplit(SplitMethod.Word, Palette.RainbowWave));

            Kon.WriteLine();
            Kon.WriteLine();

            WriteLog();

            SonyDevice dev = new SonyDevice("hub");
            dev.Alias.Add("xbox", "action");
            
            Kon.WriteLine(dev.Info.Model);
            
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

            string ascii =
                @"                      ,." + Environment.NewLine +
                @"                 .*&@@@@# .#@@@%*" + Environment.NewLine +
                @"                #@@%,.&@&@@@//@%," + Environment.NewLine +
                @"             .%@@(,  ,@@@&/  ,@#," + Environment.NewLine +
                @"           ,#@%/      ,*.    .&@@(." + Environment.NewLine +
                @"          *@@#                 *#@@@/." + Environment.NewLine +
                @"         ,@&(                     ,/&@@%" + Environment.NewLine +
                @"        ,@%                 ,/%@@@&%/." + Environment.NewLine +
                @"       *%@,               *@@@%*, ,/%&@@@@@@@@%#/." + Environment.NewLine +
                @"       %@#               /&@,.*%&@@@%(*,...,,*#&@@%(" + Environment.NewLine +
                @"      ,@@,               .(@@@@%#,               *@@%" + Environment.NewLine +
                @"     ,#@%                                          *@%," + Environment.NewLine +
                @"     ,%@%                                          ,@%," + Environment.NewLine +
                @"      ,,.   ./%&@@@@@@@@@@@@@@@&%(,.             .%@@.  /*" + Environment.NewLine +
                @"         ,%@@@%(,.          ..*#%&@@@&%(,,,.,,*#&@@%.  ,@@," + Environment.NewLine +
                @"      .#&@&*,                       ,(#&@@@@@@@&%*      &@(" + Environment.NewLine +
                @"    (@@%/                                               %@#" + Environment.NewLine +
                @"  *%@#*                                                 &@(" + Environment.NewLine +
                @" *@@/                                                  *@@," + Environment.NewLine +
                @",@@&%#(,.                                             .@&(" + Environment.NewLine +
                @",##%%&@@@#,                                         /&@*" + Environment.NewLine +
                @"         /&@@(                                      (@@*" + Environment.NewLine +
                @"            ,%@@#.                               .%@@*" + Environment.NewLine +
                @"              ,/@@@/                           /#@@#" + Environment.NewLine +
                @"                 /%@@&(.                   ./#@@&/" + Environment.NewLine +
                @"                     .(&@@@@%#/*,,,,*(#%@@@@#/." + Environment.NewLine +
                @"                         .,/%%&@@@@@&&%#*,." + Environment.NewLine;

            ascii = ascii.ForceIndent(29);

            Kon.Prefix.Current = Prefixes.Setting.None;
            
            Kon.WriteLine(ascii, NewLines.Setting.Both);
            Kon.WriteLine("SENKO (Project:REMO) | Remote Controller Interface\n<gray>Version: {0}", ConsoleColor.White, version);
            Kon.WriteLine(@"https://github.com/TsurugiDanzen/Remo" + Environment.NewLine, ConsoleColor.Gray);

            Kon.Prefix.Current = Prefixes.Setting.Auto;

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
