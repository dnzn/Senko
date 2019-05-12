namespace Kontext
{
    using System;
    using System.Collections.Generic;
    using Global;

    using static Static;
    using static Konsole.Parameters.Color;
    using System.Text.RegularExpressions;

    public static class Static
    {
        public static List<LogEntry> Log { get; } = new List<LogEntry>();
        public static List<string> Names { get; private set; } = new List<string>();

        public static void WriteLog(this Konsole konsole, bool truncate = true)
        {
            Konsole.WriteLog(konsole.Name, truncate);
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
            ReadLine
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

                bool first = true;

                foreach (string s in Split(text))
                {
                    string t = s;

                    if (color != Color.Current && !StartsWithTag(t))
                    {
                        t = GetColor(color).Encapsulate("<") + t;
                    }

                    Console.Write(Color.Paint(t));

                    if (first)
                    {
                        Log.Add(new LogEntry(Name, start, method, t));
                    }
                    else
                    {
                        Log.Add(new LogEntry(Name, start, t));
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

        public static void WriteLog(string name, bool truncate = true)
        {
            string limitName = null;

            if (name != null && Names.Contains(name))
            {
                limitName = name;
            }

            string log = "Konsole Log : " + limitName;
            log = log.AppendLine(new string('=', Console.WindowWidth - 2));

            string lastDate = "";

            foreach (LogEntry entry in Log)
            {
                string logDate = entry.Time.ToString("MM/dd/yyyy");

                if (lastDate != logDate)
                {
                    log = log.AppendLine(logDate);
                    log = log.AppendLine(new string('-', Console.WindowWidth - 2));
                    lastDate = logDate;
                }

                if (limitName == null || limitName == entry.Name)
                {
                    log = log.AppendLine(entry.ToString(limitName, truncate));
                }
            }

            log = log.AppendLine(new string('-', Console.WindowWidth - 2));

            Console.WriteLine();
            Console.WriteLine(log);
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
