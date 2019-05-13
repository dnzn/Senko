namespace Kontext
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Generic;

    using static Generic.Fields;
    using static Static;
    using static Konsole.Parameters.Color;

    public static class Static
    {
        public static List<LogEntry> Log { get; } = new List<LogEntry>();
        public static List<string> Names { get; private set; } = new List<string>();

        public static void WriteLog(this Konsole konsole, bool truncate = true)
        {
            Konsole.WriteLog(konsole, truncate);
        }

        public static string ForceIndent(this string text, int indentLength)
        {
            string Indent = new string(' ', indentLength);
            return Regex.Replace(text, Environment.NewLine + "|^", @"$&" + Indent);
        }
    }

    public partial class Konsole
    {
        public enum OperationMethod
        {
            Write,
            WriteLine,
            Read,
            ReadLine,
            WriteLog
        };

        public string Name { get; private set; }
        static int CursorPosition { get { return Console.CursorLeft; } } 

        public Konsole(string instanceName)
        {
            Name = instanceName;
            Color = new Parameters.Color(this);
            Prefix = new Parameters.Prefix(this);
            NewLine = new Parameters.NewLine(this);
            Console.CursorVisible = false;

            if (!Names.Contains(Name))
            {
                Names.Add(Name);
            }
        }

        void Echo(string text, OperationMethod method, params object[] objectArray)
        {
            DateTime start = DateTime.Now;

            if (method.ToString().Contains("Write"))
            {
                List<string> stringList = new List<string>();
                ColorSplit ColorSplitter = new ColorSplit();

                PrefixType prefix = Prefix.Current;
                NewLineType newline = (method == OperationMethod.Write) ? NewLine.Write : NewLine.WriteLine;
                ConsoleColor color = Current;

                foreach (object obj in objectArray)
                {
                    if (stringList.Count == 0)
                    {
                        if (obj is NewLineType obj_newline)
                        {
                            newline = (method == OperationMethod.Write) ? NewLine.OverrideWrite = obj_newline : NewLine.OverrideWriteLine = obj_newline;
                        }
                        else if (obj is PrefixType obj_prefix)
                        {
                            prefix = Prefix.Override = obj_prefix;
                            newline = (method == OperationMethod.Write) ? NewLine.OverrideWrite : NewLine.OverrideWriteLine;
                        }
                        else if (obj is ConsoleColor obj_color)
                        {
                            color = obj_color;
                        }
                        else if (obj is ColorSplit obj_splitParameters)
                        {
                            ColorSplitter = obj_splitParameters;
                        }
                        else
                        {
                            stringList.Add(obj.ToString());
                        }
                    }
                    else
                    {
                        stringList.Add(obj.ToString());
                    }
                }
                text = text.Format(stringList.ToArray());

                text = ColorSplitter.Execute(text);

                if (prefix == Prefix.Current)
                {
                    if (CursorPosition == 0 || (CursorPosition > 0 && (newline == NewLineType.Prepend || newline == NewLineType.Both)))
                    {
                        text = Prefix.Insert(text);
                    }
                }
                else
                {
                    text = Prefix.Insert(text, prefix);
                }

                text = NewLine.Insert(text, newline);

                bool first = true;

                foreach (string s in Split(text))
                {
                    string t = s;

                    if (color != Current && !StartsWithTag(t))
                    {
                        t = InsertTag(t, color, false);
                    }

                    Console.Write(Color.Paint(t));

                    if (method != OperationMethod.WriteLog)
                    {
                        if (first)
                        {
                            Log.Add(new LogEntry(Name, start, method, t));
                        }
                        else
                        {
                            Log.Add(new LogEntry(Name, start, t));
                        }
                    }

                    first = false;
                } 
            }

            Color.Reset();
        }

        public void Write(object obj, params object[] objectArray)
        {
            Echo(obj.ToString(), OperationMethod.Write, objectArray);
        }

        public void Write(object[] obj, params object[] objectArray)
        {
            Write(obj.Join(), objectArray);
        }

        public void WriteLine(object obj, params object[] objectArray)
        {
            Echo(obj.ToString(), OperationMethod.WriteLine, objectArray);
        }

        public void WriteLine()
        {
            Console.WriteLine();
        }

        static string HorizontalDivider(char c)
        {
            return " " + new string(c, Console.WindowWidth - 2);
        }

        public static void WriteLog(Konsole konsole, bool truncate = true)
        {    
            string limitName = null;

            if (konsole != null && Names.Contains(konsole.Name))
            {
                limitName = konsole.Name;
            }

            string horizontalDivider = InsertTag(HorizontalDivider('-'), ConsoleColor.DarkGray);
            string log = InsertTag(HorizontalDivider('='), ConsoleColor.DarkGray);
            log = log.AppendLine(InsertTag(" Konsole Log : " + ((limitName != null) ? limitName : "Full Record"), ConsoleColor.Cyan));
            log = log.AppendLine(horizontalDivider);

            int NameMaxLength = 0;
            int ElapsedMaxLength = 0;
            int OperationMaxLength = 0;

            foreach (LogEntry entry in Log)
            {
                if (limitName == null || limitName == entry.Name)
                {
                    NameMaxLength = (entry.Name.Length > NameMaxLength) ? entry.Name.Length : NameMaxLength;
                    ElapsedMaxLength = (entry.ElapsedTime.Length > ElapsedMaxLength) ? entry.ElapsedTime.Length : ElapsedMaxLength;
                    OperationMaxLength = (entry.Operation.Length > OperationMaxLength) ? entry.Operation.Length : OperationMaxLength;
                }
            }

            string lastDate = "";

            foreach (LogEntry entry in Log)
            {
                string text;
                string logDate = entry.Time.ToString("dddd, MMMM d, yyyy");
                
                if (lastDate != logDate)
                {
                    log = log.AppendLine(" <white>" + logDate);
                    log = log.AppendLine(horizontalDivider);
                    lastDate = logDate;
                }                

                if (limitName == null || limitName == entry.Name)
                {
                    string divider = "<darkgray>|<gray>";
                    int dividerLength;

                    if (limitName == null)
                    {
                        text = " {1} {0} {2} {0} {3} {0} {4} {0} {5}".Format(
                            divider,
                            entry.Time.ToString("HH:mm:ss:fff"),
                            entry.ElapsedTime.PadLeft(ElapsedMaxLength),
                            entry.Name.PadRight(NameMaxLength),
                            entry.Operation.PadRight(OperationMaxLength),
                            DisableTags(entry.Text));

                        dividerLength = (divider.Length * 4) - 4;
                    }
                    else
                    {
                        text = " {1} {0} {2} {0} {3} {0} {4} ".Format(
                            divider,
                            entry.Time.ToString("HH:mm:ss:fff"),
                            entry.ElapsedTime.PadLeft(ElapsedMaxLength),
                            entry.Operation.PadRight(OperationMaxLength),
                            DisableTags(entry.Text));
                        
                        dividerLength = (divider.Length * 3) - 3;
                    }

                    int l = Console.WindowWidth - 5; 
                    text = (truncate) ? (CleanTags(text).Length <= l) ? text : text.Substring(0, l + dividerLength) + InsertTag("[...]", ConsoleColor.DarkGray) : text;

                    log = log.AppendLine(InsertTag(text, ConsoleColor.Gray));
                }
            }

            log = log.AppendLine(horizontalDivider);

            Kon.WriteLine();

            foreach (string s in Split(log))
            {
                Console.Write(Kon.Color.Paint(s));
            }
        }

        public static void WriteLog(bool truncate = true)
        {
            WriteLog(null, truncate);
        }

        public class ColorSplit
        {
            public SplitMethod Method { get; private set; }
            public Palette Palette { get; private set; }
            public int ChunkSize { get; private set; }

            public ColorSplit(SplitMethod method, Palette? palette, int chunkSize = 0)
            {
                SetParameters(method, palette, chunkSize);
            }

            public ColorSplit(SplitMethod method)
            {
                SetParameters(method, null);
            }

            public ColorSplit(Palette palette)
            {
                SetParameters(SplitMethod.Char, palette);
            }

            public ColorSplit(int chunkSize)
            {
                SetParameters(SplitMethod.Chunk, null, chunkSize);
            }

            public ColorSplit()
            {
                SetParameters(SplitMethod.None, null);
            }

            void SetParameters(SplitMethod method, Palette? palette, int chunkSize = 0)
            {
                if (chunkSize == 0 && method == SplitMethod.Chunk)
                {
                    method = SplitMethod.Word;
                }
                else if (chunkSize > 0 && method != SplitMethod.Chunk)
                {
                    method = SplitMethod.Chunk;
                }

                Method = method;

                if (method != SplitMethod.None)
                {
                    Palette = palette ?? Palette.Auto;

                    if (chunkSize > 0)
                    {
                        ChunkSize = chunkSize;
                    }
                }
            }

            public string Execute(string text)
            {
                if (Method != SplitMethod.None)
                {
                    switch (Method)
                    {
                        case SplitMethod.Line:
                            text = PaletteLines(text, Palette).Join();
                            break;
                        case SplitMethod.Word:
                            text = PaletteWords(text, Palette).Join();
                            break;
                        case SplitMethod.Chunk:
                            text = PaletteChunks(text, Palette, ChunkSize).Join();
                            break;
                        case SplitMethod.Char:
                            text = PaletteChars(text, Palette).Join();
                            break;
                    }
                }

                return text;
            }
        }
    }
}
