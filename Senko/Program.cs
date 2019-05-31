namespace Senko
{
    using System;
    using System.Diagnostics;
    using Generic;
    using Polymer;

    using static Polymer.Konsole;
    using static Polymer.Kontext;
    using static Polymer.Konsole.Parameters;
    using static Generic.Fields;
    using static Generic.Extensions;
    using System.Text.RegularExpressions;

    class Program
    {
        static void Main(string[] args)
        {
            Welcome();

            var dev = new SonyDevice("hub");
            dev.Alias.Add("xbox", "action");
            
            Kon.WriteLine("Device Model: " + dev.Info.Model);
            Kon.WriteLine("HDMI IRCode: " + dev.Command.Code["Hdmi1"]);
            Kon.WriteLine("Netflix: " + dev.Apps.List["Netflix"]);
            Kon.WriteLine("<test><tags>abcdefg\n<testagain>");
            Kon.WriteLine("Write this with and Indent prefix. Let's see if it works as I expect the program to work.", PrefixType.Indent);

            Kon.WriteLine("The quick brown fox jumps over the lazy dog.");
            Kon.WriteLine("The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. ");
            Kon.WriteLine("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789");
            
            //WriteLog();

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
            // Senko is also a backronym for "SENd KOmmand".

            string title = Color.InsertTag("PROJECT:SENKO | Remote Control", ConsoleColor.Cyan);

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = "Version: " + fvi.ProductVersion;
            version = Color.InsertTag(version.Join("@", 43 - version.Length, "#"), ConsoleColor.White);

            string github = Color.InsertTag(@"https://github.com/TsurugiDanzen/Senko", ConsoleColor.White);

            var ascii = new string[]
            {
                "@26#,.",
                "@17#.*&&4@# .#@@@%*",
                "@16##@@%,.&@&@@@//@%,",
                "@13#.%@@(,  ,@@@&/  ,@#,",
                "@11#,#@%/@6#,*.@4#.&@@(.@15#",
                "@10#*@@#@17#*#@@@/.",
                "@9#,@&(@21#,/&@@%",
                "@8#,@%@17#,/%@@@&%/.",
                "@7#*%@,@15#*@@@%*, ,/%&&8@}%#/.",
                "@7#%@#@15#/&@,.*%&@@@%(*,...,,*#&@@%(",
                "@6#,@@,@15#.(&4@%#,@15#*@@%",
                "@5#,#@%@42#*@%,",
                "@5#,%@%@42#,@%,",
                "@6#,,.   ./%&&15@&%(,.@13#.%@@.  /*",
                "@9#,%@@@%(,.@10#..*#%&@@@&%(,,,.,,*#&@@%.  ,@@,",
                "@6#.#&@&*,@23#,(#&&7@&%*@6#&@(",
                "@4#(@@%/@47#%@#",
                "  *%@#*@5#{0}@14#&@(",
                " *@@/@7#{1}*@@,",
                ",@@&%#(,.   {2}@4#.@&(",
                ",##%%&@@@#,@42#/&@*",
                Color.InsertTag(new string('#', Console.WindowWidth - 1), ConsoleColor.DarkGray, false) + Environment.NewLine
                //@"@9#/&@@(@38#(@@*",
                //@"@12#,%@@#.@31#.%@@*",
                //@"@14#,/@@@/@27#/#@@#",
                //@"@17#/%@@&(.@19#./#@@&/",
                //@"@21#.(&&4@%#/*&4,*(#%&4@#/.",
                //@"@25#.,/%%&&5&&%#*,." + Environment.NewLine
            }.Join(Environment.NewLine);

            Kon.WriteLine(ascii.Decompress(), WordWrap.Disabled, NewLineType.Both, ConsoleColor.Gray, PrefixType.None, title, version.Decompress(), github);
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
