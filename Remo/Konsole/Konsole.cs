namespace Remo
{
    using System;
    using System.Collections.Generic;
    using static Konsole.Colors;

    public partial class Konsole
    {
        public enum OperationMethod
        {
            Write,
            WriteLine,
            Read,
            ReadLine
        };

        public string InstanceName { get; private set; }
        public static List<LogEntry> Log { get; } = new List<LogEntry>();
        int CursorPosition { get { return Console.CursorLeft; } } 

        public Konsole(string instanceName)
        {
            InstanceName = instanceName;
            Color = new Colors(this);
            Prefix = new Prefixes(this);
            NewLine = new NewLines(this);
            Console.CursorVisible = false;
        }

        void Print(string text, OperationMethod method, params object[] objectArray)
        {
            if (method.ToString().Contains("Write"))
            {
                List<string> stringList = new List<string>();
                ColorSplit ColorSplitter = new ColorSplit();

                Prefixes.Setting prefix = Prefix.Current;
                NewLines.Setting newline = (method == OperationMethod.Write) ? NewLine.Write : NewLine.WriteLine;

                foreach (object obj in objectArray)
                {
                    if (stringList.Count == 0)
                    {
                        if (obj is NewLines.Setting obj_newline)
                        {
                            newline = (method == OperationMethod.Write) ? NewLine.OverrideWrite = obj_newline : NewLine.OverrideWriteLine = obj_newline;
                        }
                        else if (obj is Prefixes.Setting obj_prefix)
                        {
                            prefix = Prefix.Override = obj_prefix;
                            newline = (method == OperationMethod.Write) ? NewLine.OverrideWrite : NewLine.OverrideWriteLine;
                        }
                        else if (obj is ConsoleColor obj_color)
                        {
                            if (!ContainsTag(text))
                            {
                                text = InsertTag(text, obj_color);
                            }
                            else
                            {
                                if (StartsWithTag(text))
                                {
                                    text = ReplaceTag(text, GetColor(obj_color).Encapsulate("<"), 1);
                                }
                                else
                                {
                                    text = InsertTag(text, obj_color);
                                }
                            }
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
                    if (CursorPosition == 0 || (CursorPosition > 0 && (newline == NewLines.Setting.Prepend || newline == NewLines.Setting.Both)))
                    {
                        text = Prefix.Insert(text);
                    }
                }
                else
                {
                    text = Prefix.Insert(text, prefix);
                }

                text = NewLine.Insert(text, newline);

                Log.Add(new LogEntry(InstanceName, method, text));

                foreach (string colorSplit in Split(text))
                {
                    Console.Write(Color.Paint(colorSplit));
                } 
            }
        }

        public void Write(object obj, params object[] objectArray)
        {
            Print(obj.ToString(), OperationMethod.Write, objectArray);
        }

        public void Write(object[] obj, params object[] objectArray)
        {
            Write(obj.Join(), objectArray);
        }

        public void WriteLine(object obj, params object[] objectArray)
        {
            Print(obj.ToString(), OperationMethod.WriteLine, objectArray);
        }

        public void WriteLine()
        {
            Console.WriteLine();
        }

        public void WriteLog(bool truncate = false)
        {
            Color.Toggle();
            Color.Switch();

            foreach (LogEntry log in Log)
            {
                Console.WriteLine(log.ToString(truncate));
            }

            Color.Toggle();
        }

        public class ColorSplit
        {
            public SplitMethod Method { get; private set; }
            public Palette Palette { get; private set; }
            public int ChunkSize { get; private set; }

            public ColorSplit(SplitMethod method, Palette? palette, int chunkSize)
            {
                SetParameters(method, palette, chunkSize);
            }

            public ColorSplit(SplitMethod method, Palette palette)
            {
                SetParameters(method, palette);
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
