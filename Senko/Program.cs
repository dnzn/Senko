namespace Senko
{
    using System;
    using System.Diagnostics;
    using Generic;

    using static Kontext.Konsole;
    using static Kontext.Konsole.Parameters;
    using static Generic.Fields;
    using static Generic.Extensions;

    class Program
    {
        static void Main(string[] args)
        {
            Welcome();

            SonyDevice dev = new SonyDevice("hub");
            dev.Alias.Add("xbox", "action");
            
            Kon.WriteLine(dev.Info.Model);
            Kon.WriteLine(dev.Command.Code["Hdmi1"]);

            WriteLog();

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

            string title = Color.InsertTag("PROJECT:SENKO | Remote Control", ConsoleColor.Cyan);
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = "Version: " + fvi.ProductVersion;
            int l = 43 - version.Length;
            version = Color.InsertTag(version.Join("@", l, "#"), ConsoleColor.White);
            string github = Color.InsertTag(@"https://github.com/TsurugiDanzen/Senko", ConsoleColor.White);

            string ascii = new string[]
            {
                @"@22#,.",
                @"@17#.*&&4@# .#@@@%*",
                @"@16##@@%,.&@&@@@//@%,",
                @"@13#.%@@(,  ,@@@&/  ,@#,",
                @"@11#,#@%/@6#,*.@4#.&@@(.@15#",
                @"@10#*@@#@17#*#@@@/.",
                @"@9#,@&(@21#,/&@@%",
                @"@8#,@%@17#,/%@@@&%/.",
                @"@7#*%@,@15#*@@@%*, ,/%&&8@}%#/.",
                @"@7#%@#@15#/&@,.*%&@@@%(*,...,,*#&@@%(",
                @"@6#,@@,@15#.(&4@%#,@15#*@@%",
                @"@5#,#@%@42#*@%,",
                @"@5#,%@%@42#,@%,",
                @"@6#,,.   ./%&&15@&%(,.@13#.%@@.  /*",
                @"@9#,%@@@%(,.@10#..*#%&@@@&%(,,,.,,*#&@@%.  ,@@,",
                @"@6#.#&@&*,@23#,(#&&7@&%*@6#&@(",
                @"@4#(@@%/@47#%@#",
                @"  *%@#*@5#{0}@14#&@(",
                @" *@@/@7#{1}*@@,",
                @",@@&%#(,.   {2}@4#.@&(",
                @",##%%&@@@#,@42#/&@*",
                Color.InsertTag(new string('#', Console.WindowWidth - 1), ConsoleColor.DarkGray) + Environment.NewLine
                //@"@9#/&@@(@38#(@@*",
                //@"@12#,%@@#.@31#.%@@*",
                //@"@14#,/@@@/@27#/#@@#",
                //@"@17#/%@@&(.@19#./#@@&/",
                //@"@21#.(&&4@%#/*&4,*(#%&4@#/.",
                //@"@25#.,/%%&&5&&%#*,." + Environment.NewLine
            }.Join(Environment.NewLine);

            Kon.WriteLine(ascii.Decompress(), NewLineType.Both, ConsoleColor.Gray, PrefixType.None, title, version.Decompress(), github);
            Kon.WriteLine();

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
