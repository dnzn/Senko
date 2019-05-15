namespace Kontext
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Generic;

    using static Generic.Fields;
    using static Static;
    using static Konsole.Parameters.Color;
    using System.Linq;

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
            WriteLog,
            Previous
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

                    if (first)
                    {
                        Log.Add(new LogEntry(this, start, method, t));
                    }
                    else
                    {
                        Log.Add(new LogEntry(this, start, OperationMethod.Previous, t));
                    }

                    start = DateTime.Now;

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
            DateTime start = DateTime.Now;
            
            string limitName = null;

            if (konsole != null && Names.Contains(konsole.Name))
            {
                limitName = konsole.Name;
            }

            ConsoleColor tableColor = ConsoleColor.DarkGray;

            string whiteTag = CreateTag(ConsoleColor.White);
            string grayTag = CreateTag(ConsoleColor.Gray);
            string darkCyanTag = CreateTag(ConsoleColor.DarkCyan);

            string thickDivider = InsertTag(HorizontalDivider('='), ConsoleColor.DarkCyan);
            string thinDivider = HorizontalDivider('-');
            string log = thickDivider;

            string headerText = " [KONSOLE LOG] : " + ((limitName != null) ? limitName : "Full Record");
            string logStart = "LOG START";

            log = log.AppendLine(CreateTag(ConsoleColor.Cyan) + headerText + new string(' ', Console.WindowWidth - headerText.Length - logStart.Length - 1) + darkCyanTag + logStart);
            log = log.AppendLine(darkCyanTag + thinDivider);
            
            string lastDate = "";
            int counter = 0;
            int records = 1;

            foreach (LogEntry entry in Log)
            {
                string row;
                counter++;

                string logDate = entry.Time.ToString("dddd, MMMM d, yyyy");                
                
                if (lastDate != logDate)
                {
                    log = log.AppendLine(" " + whiteTag + logDate);
                    log = log.AppendLine(CreateTag(tableColor) + thinDivider);
                    lastDate = logDate;
                }

                string record = "";
                int recordLength = LogEntry.Records.ToString().Length;

                if (entry.Operation != OperationMethod.Previous)
                {
                    record = records.ToString(new string('#', recordLength)).PadRight(recordLength);
                    records++;
                }
                else
                {
                    record = new string(' ', recordLength);
                }

                string time = entry.Time.ToString("HH:mm:ss:fff");
                string elapsedTime = entry.ElapsedTime.PadLeft(LogEntry.ElapsedMaxLength);
                string name = entry.Name.PadRight(LogEntry.NameMaxLength);
                string operation = ((entry.Operation != OperationMethod.Previous) ? entry.Operation.ToString() : "").PadRight(LogEntry.OperationMaxLength);
                string text = (entry.Operation == OperationMethod.WriteLog) ? entry.Text.Encapsulate("[") : entry.Text;

                string colorTag = (entry.Operation != OperationMethod.Previous) ? whiteTag : grayTag;

                if (limitName == null || limitName == entry.Name)
                {
                    string divider = WrapWithTag(" | ", @"\|", tableColor, colorTag);
                    List<string> list;

                    list = (limitName == null) ?
                        new List<string>() { record, name, time, elapsedTime, operation, DisableTags(text) } :
                        new List<string>() { record, time, elapsedTime, operation, DisableTags(text) };

                    int logCountLength = Log.Count.ToString().Length;

                    row = " " + counter.ToString(new string('#', logCountLength)).PadLeft(logCountLength);

                    for (int j = 0; j < list.Count; j++)
                    {
                        row += divider + list[j];
                    }

                    if (truncate && CleanTags(row).Length > Console.WindowWidth)
                    {
                        int dividerLength = ((divider.Length * list.Count) - list.Count) - ((CleanTags(divider).Length * list.Count) - list.Count);
                        int trimLength = Console.WindowWidth + dividerLength;
                        int textLength = LogEntry.TextMaxLength.ToString().Length;
                        string trimmed = row.Substring(trimLength - textLength - 3);
                        string endMark = ("+" + (trimmed.Length - trimmed.Count(@"\n"))).PadLeft(textLength + 1, '.').Encapsulate("[");
                        row = row.Substring(0, trimLength - endMark.Length) + InsertTag(endMark, tableColor);
                    }

                    row = colorTag + WrapWithTag(row, @"\\n|<\\\w+>", tableColor, colorTag);

                    log = log.AppendLine(row);
                }
            }

            Kon.WriteLine();

            Paint(log);

            Log.Add(new LogEntry(konsole, start, OperationMethod.WriteLog, Log.Count + ((Log.Count > 1) ? " log entries were written..." : " log entry was written...")));

            string processTime = " WriteLog process completed in {0} for {1} {2}".Format(Log.Last().ElapsedTime, counter, (counter > 1) ? "entries." : "entry.");
            string logEnd = "LOG END";
            string footer = darkCyanTag + thinDivider;
            footer = footer.AppendLine(darkCyanTag + processTime + new string(' ', Console.WindowWidth - logEnd.Length - processTime.Length - 1) + logEnd);
            footer = footer.AppendLine(thickDivider);

            Paint(footer);
        }

        public static void WriteLog(bool truncate = true)
        {
            WriteLog(null, truncate);
        }

        static void Paint(string text, bool writeline = true)
        {
            foreach (string s in Split(text))
            {
                Console.Write(Kon.Color.Paint(s));
            }

            if (writeline)
            {
                Console.WriteLine();
            }
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
