namespace Remo
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using static Global;

    public partial class Konsole
    {
        public enum OperationMethod { Write, WriteLine, Read, ReadLine };
        public string Name { get; private set; }

        public static bool Verbose { get; set; } = true;
        public static List<LogEntry> Log { get; } = new List<LogEntry>();

        public Konsole(string name)
        {
            Name = name;
            Color = new Colors(this);
            Prefix = new Prefixes(this);
            NewLine = new NewLines(this);
        }

        void Print(string text, OperationMethod method, params object[] p)
        {
            int cursorPosition = Console.CursorLeft;

            Prefixes.Setting prefix = Prefix.Current;
            Prefixes.Setting? prefixOverride = null;
            NewLines.Setting newline;
            NewLines.Setting? newlineOverride = null;

            switch (method)
            {
                case OperationMethod.Write:
                    newline = NewLine.Write;
                    break;
                default:
                    newline = NewLine.WriteLine;
                    break;
            }            

            Type lastParameterType;

            Colors.Palette? nullPalette = null;
            Colors.SplitMethod splitMethod = Colors.SplitMethod.Char;
            int chunkSize = 0;

            lastParameterType = null;

            List<string> stringParams = new List<string>();

            foreach (object o in p)
            {
                if (lastParameterType != typeof(string))
                {
                    if (o is NewLines.Setting _newline)
                    {
                        newlineOverride = (method == OperationMethod.Write) ? NewLine.OverrideWrite = _newline : NewLine.OverrideWriteLine = _newline;

                        lastParameterType = o.GetType();
                    }
                    else if (o is Prefixes.Setting _prefix)
                    {
                        prefixOverride = Prefix.Override = _prefix;
                        newlineOverride = (method == OperationMethod.Write) ? NewLine.OverrideWrite : NewLine.OverrideWriteLine;

                        lastParameterType = o.GetType();
                    }
                    else if (o is ConsoleColor _color)
                    {
                        if (!Colors.ContainsTag(text))
                        {
                            text = Colors.InsertTag(text, _color);
                        }
                        else
                        {
                            if (Colors.StartsWithTag(text))
                            {
                                text = Colors.ReplaceTag(text, Colors.GetColor(_color).Encapsulate("<"), 1);
                            }
                            else
                            {
                                text = Colors.InsertTag(text, _color);
                            }
                        }

                        lastParameterType = o.GetType();
                    }
                    else if (o is Colors.Palette _palette)
                    {
                        nullPalette = _palette;

                        lastParameterType = o.GetType();
                    }
                    else if (o is Colors.SplitMethod _splitmethod)
                    {
                        if (lastParameterType == typeof(Colors.Palette))
                        {
                            splitMethod = _splitmethod;
                        }
                    }
                    else if (o is int x)
                    {
                        if (lastParameterType == typeof(Colors.Palette) && splitMethod == Colors.SplitMethod.Chunk)
                        {
                            chunkSize = x;
                        }
                        else
                        {
                            string s = o.ToString();

                            stringParams.Add(s);

                            lastParameterType = s.GetType();
                        }
                    }
                    else
                    {
                        string s = o.ToString();

                        stringParams.Add(s);

                        lastParameterType = s.GetType();
                    }
                }
                else
                {
                    stringParams.Add(o.ToString());
                }
            }

            text = text.Format(stringParams.ToArray());

            if (nullPalette != null && splitMethod != Colors.SplitMethod.None)
            {
                Colors.Palette palette = nullPalette ?? Colors.Palette.All;

                if (chunkSize == 0 && splitMethod == Colors.SplitMethod.Chunk)
                {
                    splitMethod = Colors.SplitMethod.Word;
                }
                else if (chunkSize > 0 && splitMethod != Colors.SplitMethod.Chunk)
                {
                    splitMethod = Colors.SplitMethod.Chunk;
                }

                switch (splitMethod)
                {
                    case Colors.SplitMethod.Line:
                        text = Colors.PaletteLines(text, palette).Join();
                        break;
                    case Colors.SplitMethod.Word:
                        text = Colors.PaletteWords(text, palette).Join();
                        break;
                    case Colors.SplitMethod.Chunk:
                        text = Colors.PaletteChunks(text, palette, chunkSize).Join();
                        break;
                    case Colors.SplitMethod.Char:
                        text = Colors.PaletteChars(text, palette).Join();
                        break;
                    default:
                        break;
                }
            }

            if (prefixOverride == null)
            {
                if (cursorPosition == 0 || (cursorPosition > 0 && (newline == NewLines.Setting.Prepend || newline == NewLines.Setting.Both)))
                {
                    text = Prefix.Insert(text);
                }
            }
            else
            {
                text = Prefix.Insert(text, prefixOverride ?? prefix);
            }

            text = NewLine.Insert(text, newlineOverride ?? newline);

            Log.Add(new LogEntry(Name, method, text));
            
            foreach (string s in Colors.Split(text))
            {
                Console.Write(Color.Paint(s));
            }
        }

        public void Write(object obj, params object[] p)
        {
            Print(obj.ToString(), OperationMethod.Write, p);
        }

        public void Write(object[] obj, params object[] p)
        {
            Write(obj.Join(), p);
        }

        public void WriteLine(object obj, params object[] p)
        {
            Print(obj.ToString(), OperationMethod.WriteLine, p);
        }

        public void WriteLine()
        {
            Console.WriteLine();
        }
    }
}
