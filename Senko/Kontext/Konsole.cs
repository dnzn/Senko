namespace Kontext
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Generic;
    using System.Linq;

    using static Generic.Fields;
    using static Kontext;
    using static Konsole.Parameters;

    public static class Kontext
    {
        public enum OperationMethod
        {
            Write,
            WriteLine,
            Read,
            ReadLine,
            WriteLog,
            PreviousOperation
        };

        public static Konsole MainKonsole { get; set; }
        public static List<Konsole> Konsoles { get; } = new List<Konsole>();

        public static Regex RegexForceIndent { get; } = new Regex(Environment.NewLine + "|^");

        public static void WriteLog(this Konsole konsole, bool truncate = true)
        {
            Konsole.WriteLog(konsole, truncate);
        }

        public static string ForceIndent(this string text, int indentLength)
        {
            var Indent = new string(' ', indentLength);
            return RegexForceIndent.Replace(text, @"$&" + Indent);
        }

        public static bool Contains(this List<Konsole> konsoles, string name)
        {
            foreach (Konsole konsole in konsoles)
            {
                if (konsole.Name == name)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public partial class Konsole
    {
        public string Name { get; private set; }
        static int CursorPosition { get { return Console.CursorLeft; } } 

        public Konsole(string instanceName)
        {
            Name = instanceName;
            Color = new Parameters.Color(this);
            Prefix = new Parameters.Prefix(this);
            NewLine = new Parameters.NewLine(this);
            Console.CursorVisible = false;

            if (MainKonsole == null)
            {
                MainKonsole = this;
            }

            if (!Konsoles.Contains(Name))
            {
                Konsoles.Add(this);
            }
        }

        void Echo(string text, OperationMethod method, params object[] objectArray)
        {
            Log.Initialize(this, method);

            if (method.ToString().Contains("Write"))
            {
                var stringList = new List<string>();
                var ColorSplitter = new ColorSplit();

                PrefixType prefix = Prefix.Current;
                NewLineType newline = (method == OperationMethod.Write) ? NewLine.Write : NewLine.WriteLine;
                ConsoleColor color = Color.Current;

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

                Painter(text, this, color, method);
            }

            Color.Reset();
        }

        static void Painter(string text, Konsole konsole, ConsoleColor color, OperationMethod method)
        {
            bool first = true;

            foreach (string s in Color.Split(text))
            {
                string t = s;

                if (color != Color.Current && !Color.StartsWithTag(t))
                {
                    t = Color.InsertTag(t, color, false);
                }

                Console.Write(konsole.Color.Paint(t));

                if (first)
                {
                    Log.Commit(t);
                }
                else
                {
                    Log.Commit(OperationMethod.PreviousOperation, t);
                }

                first = false;
            }
        }

        static void Painter(string text, Konsole konsole, bool writeline = true)
        {
            foreach (string s in Color.Split(text))
            {
                Console.Write(MainKonsole.Color.Paint(s));
            }

            if (writeline)
            {
                Console.WriteLine();
            }
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
            Log.Initialize(konsole, OperationMethod.WriteLog);

            Konsole painter;
            List<Log.Record> Records;
            Log.MaxLengths maxLengths;

            if (konsole != null)
            {
                painter = konsole;
                Records = Log.FilterRecords(konsole);
                maxLengths = new Log.MaxLengths(Records);
            }
            else
            {
                painter = MainKonsole;
                Records = Log.Records;
                maxLengths = Log.MaxLength;
            }

            ConsoleColor tableColor = ConsoleColor.DarkGray;

            string whiteTag = Color.CreateTag(ConsoleColor.White);
            string grayTag = Color.CreateTag(ConsoleColor.Gray);
            string darkCyanTag = Color.CreateTag(ConsoleColor.DarkCyan);

            string thickDivider = Color.InsertTag(HorizontalDivider('='), ConsoleColor.DarkCyan);
            string thinDivider = HorizontalDivider('-');
            string log = thickDivider;

            string headerText = " [KONSOLE LOG] : " + ((konsole != null) ? konsole.Name : "Full Record");
            string logStart = "LOG START";

            log = log.AppendLine(Color.CreateTag(ConsoleColor.Cyan) + headerText + new string(' ', Console.WindowWidth - headerText.Length - logStart.Length - 1) + darkCyanTag + logStart);
            log = log.AppendLine(darkCyanTag + thinDivider);
            
            string lastDate = "";

            foreach (Log.Record record in Records)
            {
                string row;

                string logDate = record.StartTime.ToString("dddd, MMMM d, yyyy");                
                
                if (lastDate != logDate)
                {
                    log = log.AppendLine(" " + whiteTag + logDate);
                    lastDate = logDate;
                }

                string id = "";

                int idLength = maxLengths.OperationID + maxLengths.IterationID + 1;

                if (record.Operation != OperationMethod.PreviousOperation)
                {
                    log = log.AppendLine(Color.CreateTag(tableColor) + thinDivider);
                    string operationID = record.OperationID.ToString(new string('#', maxLengths.OperationID)).PadLeft(maxLengths.OperationID);
                    string iterationID = record.IterationID.ToString(new string('#', maxLengths.IterationID)).PadRight(maxLengths.IterationID);
                    id = operationID + "." + iterationID;
                }
                else
                {
                    id = ("." + record.IterationID.ToString(new string('#', maxLengths.IterationID).PadRight(maxLengths.IterationID))).PadLeft(idLength);
                }

                string time = record.StartTime.ToString("HH:mm:ss:fff");
                string elapsedTime = record.ElapsedTime.PadLeft(maxLengths.ElapsedTime);
                string name = record.Name.PadRight(maxLengths.Name);
                string operation = record.OperationString.PadRight(maxLengths.Operation);
                string text = (record.Operation == OperationMethod.WriteLog) ? record.Text.Encapsulate(Encapsulator.Brackets) : record.Text;

                string colorTag = (record.Operation != OperationMethod.PreviousOperation) ? whiteTag : grayTag;

                if (konsole == null || konsole.Name == record.Name)
                {
                    string divider = Color.InsertTag(" | ", @"\|", tableColor, colorTag);
                    List<string> list = (konsole == null) ?
                        new List<string>() { id, name, time, elapsedTime, operation, Color.DisableTags(text) } :
                        new List<string>() { id, time, elapsedTime, operation, Color.DisableTags(text) };

                    int logCountLength = Records.Count.ToString().Length;

                    row = " " + record.ProcessID.ToString(new string('#', logCountLength)).PadLeft(logCountLength);

                    for (int j = 0; j < list.Count; j++)
                    {
                        row += divider + list[j];
                    }

                    if (truncate && Color.CleanTags(row).Length > Console.WindowWidth)
                    {
                        int dividerLength = ((divider.Length * list.Count) - list.Count) - ((Color.CleanTags(divider).Length * list.Count) - list.Count);
                        int trimLength = Console.WindowWidth + dividerLength;
                        int textLength = maxLengths.Text.ToString().Length;
                        string trimmed = row.Substring(trimLength - textLength - 3);
                        string endMark = ("+" + (trimmed.Length - trimmed.Count(@"\n"))).PadLeft(textLength + 1, ' ').Encapsulate(Encapsulator.Brackets);
                        row = row.Substring(0, trimLength - endMark.Length) + Color.InsertTag(endMark, tableColor);
                    }

                    row = colorTag + Color.InsertTag(row, @"\\n|<\\\w+>", tableColor, colorTag);

                    log = log.AppendLine(row);
                }
            }

            Kon.WriteLine();

            Painter(log, painter);

            int entries = Records.Count;

            Log.Commit(entries + ((entries > 1) ? " log entries were written..." : " log entry was written..."));

            string processTime = " WriteLog process completed in {0} for {1} {2}".Format(Log.Records.Last().ElapsedTime, entries, (entries > 1) ? "entries." : "entry.");
            string logEnd = "LOG END";
            string footer = darkCyanTag + thinDivider;
            footer = footer.AppendLine(darkCyanTag + processTime + new string(' ', Console.WindowWidth - logEnd.Length - processTime.Length - 1) + logEnd);
            footer = footer.AppendLine(thickDivider);

            Painter(footer, painter);
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
                            text = Color.PaletteLines(text, Palette).Join();
                            break;
                        case SplitMethod.Word:
                            text = Color.PaletteWords(text, Palette).Join();
                            break;
                        case SplitMethod.Chunk:
                            text = Color.PaletteChunks(text, Palette, ChunkSize).Join();
                            break;
                        case SplitMethod.Char:
                            text = Color.PaletteChars(text, Palette).Join();
                            break;
                    }
                }

                return text;
            }
        }
    }
}
