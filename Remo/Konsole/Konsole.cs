namespace Remo
{
    using System;
    using System.Collections.Generic;

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
        public static List<LogEntry> Log { get; } = new List<LogEntry>();

        public Konsole(string name)
        {
            Name = name;
            Color = new Colors(this);
            Prefix = new Prefixes(this);
            NewLine = new NewLines(this);
        }

        void Print(string text, OperationMethod method, params object[] objectArray)
        {
            if (method.ToString().Contains("Write"))
            {
                List<string> stringList = new List<string>();
                int chunkSize = 0;
                int cursorPosition = Console.CursorLeft;
                Type lastParameterType = null;

                Colors.SplitMethod splitMethod = Colors.SplitMethod.Char;
                Colors.Palette? nullPalette = null;
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

                foreach (object obj in objectArray)
                {
                    if (lastParameterType != typeof(string))
                    {
                        if (obj is NewLines.Setting obj_newline)
                        {
                            newlineOverride = (method == OperationMethod.Write) ? NewLine.OverrideWrite = obj_newline : NewLine.OverrideWriteLine = obj_newline;

                            lastParameterType = obj.GetType();
                        }
                        else if (obj is Prefixes.Setting obj_prefix)
                        {
                            prefixOverride = Prefix.Override = obj_prefix;
                            newlineOverride = (method == OperationMethod.Write) ? NewLine.OverrideWrite : NewLine.OverrideWriteLine;

                            lastParameterType = obj.GetType();
                        }
                        else if (obj is ConsoleColor obj_color)
                        {
                            if (!Colors.ContainsTag(text))
                            {
                                text = Colors.InsertTag(text, obj_color);
                            }
                            else
                            {
                                if (Colors.StartsWithTag(text))
                                {
                                    text = Colors.ReplaceTag(text, Colors.GetColor(obj_color).Encapsulate("<"), 1);
                                }
                                else
                                {
                                    text = Colors.InsertTag(text, obj_color);
                                }
                            }

                            lastParameterType = obj.GetType();
                        }
                        else if (obj is Colors.Palette obj_palette)
                        {
                            nullPalette = obj_palette;

                            lastParameterType = obj.GetType();
                        }
                        else if (obj is Colors.SplitMethod obj_splitmethod)
                        {
                            if (lastParameterType == typeof(Colors.Palette))
                            {
                                splitMethod = obj_splitmethod;
                            }
                        }
                        else if (obj is int x)
                        {
                            if (lastParameterType == typeof(Colors.Palette) && splitMethod == Colors.SplitMethod.Chunk)
                            {
                                chunkSize = x;
                            }
                            else
                            {
                                stringList.Add(obj.ToString());

                                lastParameterType = typeof(string);
                            }
                        }
                        else
                        {
                            stringList.Add(obj.ToString());

                            lastParameterType = typeof(string);
                        }
                    }
                    else
                    {
                        stringList.Add(obj.ToString());
                    }
                }

                text = text.Format(stringList.ToArray());

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

                foreach (string colorSplit in Colors.Split(text))
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
    }
}
