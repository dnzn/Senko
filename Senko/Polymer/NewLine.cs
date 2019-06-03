namespace Polymer
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using static Static;
    using static Konsole;
    using static Polymer;
    using static Konsole.Parameters;

    public static partial class Polymer
    {
        public static void Add(this List<string> list, object obj, ConsoleColor? color, NewLineType newLineType = NewLineType.None)
        {
            string text = (color != null) ? obj.InsertTag((ConsoleColor)color) : obj.ToString() ;

            if (newLineType == NewLineType.Append || newLineType == NewLineType.Both)
            {
                text += Environment.NewLine;
            }

            if (newLineType == NewLineType.Prepend || newLineType == NewLineType.Both)
            {
                text = Environment.NewLine + text;
            }

            list.Add(text);
        }

        public static void Add(this List<string> list, object obj, NewLineType newLineType = NewLineType.None)
        {
            list.Add(obj, null, newLineType);
        }
               
        public static int MaxLength(this Konsole konsole, PrefixType prefixType)
        {
            return NewLine.MaxLength(konsole.Prefix.GetPrefix(prefixType));
        }
    }

    public partial class Konsole
    {
        public enum NewLineType
        {
            None,
            Prepend,
            Append,
            Both
        }

        public enum LongTextFormatting
        {
            Disabled,
            WordWrap,
            Truncate
        }        

        public Parameters.NewLine NewLine { get; private set; }

        public partial class Parameters
        {
            public class NewLine
            {
                static Regex RegexFlush { get; } = new Regex(@"[\r\n]+", RegexOptions.Compiled);
                static Regex RegexWordWrap { get; } = new Regex(@"\S+\s*", RegexOptions.Compiled);

                Konsole Parent { get; set; }
                public NewLineType Former { get; set; } = NewLineType.None;

                NewLineType _Write = NewLineType.None;
                public NewLineType Write
                {
                    get => _Write;
                    set
                    {
                        Former = Write;
                        _Write = value;
                    }
                }

                public NewLineType WriteLine
                {
                    get => ToWriteLine(Write);
                    set { Write = value; }
                }

                NewLineType _Override = NewLineType.None;
                public NewLineType OverrideWrite
                {
                    get
                    {
                        NewLineType _temp = _Override;
                        _Override = Write;
                        return _temp;
                    }
                    set { _Override = value; }
                }

                public NewLineType OverrideWriteLine
                {
                    get => ToWriteLine(OverrideWrite);
                    set { _Override = value; }
                }

                public NewLine(Konsole parent)
                {
                    Parent = parent;
                }

                NewLineType ToWriteLine(NewLineType setting)
                {
                    switch (setting)
                    {
                        case NewLineType.None:
                            return NewLineType.Append;
                        case NewLineType.Prepend:
                            return NewLineType.Both;
                        default:
                            return Write;
                    }
                }

                /// <summary>
                /// Insert a newline to the text as per the setting
                /// </summary>
                /// <param name="text">The text to modify</param>
                /// <param name="newLineType">The setting the apply</param>
                /// <returns>The modified text</returns>
                public string Insert(string text, NewLineType? newLineType = null)
                {
                    if (newLineType == null)
                    {
                        newLineType = Write;
                    }

                    text = ((newLineType == NewLineType.Prepend || newLineType == NewLineType.Both) && CursorLeft != 0) ? Environment.NewLine + text : text;
                    text = (newLineType == NewLineType.Append || newLineType == NewLineType.Both) ? text + Environment.NewLine : text;

                    return text;
                }

                public static string Standardize(string text)
                {
                    return RegexFlush.Replace(text, Environment.NewLine);
                }

                public static string[] Split(string text)
                {
                    text = Standardize(text);

                    return text.Split(Environment.NewLine);
                }

                public static int MaxLength(int prefixLength)
                {
                    return WindowWidth - 1 - prefixLength;
                }

                public static int MaxLength(string prefix)
                {
                    return MaxLength(prefix.Length);
                }

                public static int MaxLength()
                {
                    return MaxLength(1);
                }

                public static string FormatLongText(string text, LongTextFormatting format, string prefix = "")
                {
                    switch (format)
                    {
                        case LongTextFormatting.WordWrap:
                            return WordWrap(text, prefix);
                        case LongTextFormatting.Truncate:
                            return Truncate(text, prefix);
                        default:
                            return text;
                    }
                }

                public static string Truncate(string text, string prefix = "")
                {
                    int idealLength = MaxLength(prefix);
                    int maxLength = idealLength + text.Length - Color.CleanTags(text).Length;
                    string trimmed = "";
                    string overflow = ("+" + text.Substring(maxLength).Length).Encapsulate(EncapsulatorType.Brackets);
                    string excess;

                    while (Color.CleanTags(trimmed).Length != idealLength)
                    {
                        excess = text.Substring(maxLength - overflow.Length);
                        overflow = ("+" + (excess.Length)).Encapsulate(EncapsulatorType.Brackets);
                        trimmed = text.Substring(0, maxLength - overflow.Length) + Color.CreateTag(ConsoleColor.DarkGray) + overflow;

                        if (Color.CleanTags(trimmed).Length > idealLength)
                        {
                            maxLength--;
                        }
                        else
                        {
                            maxLength++;
                        }
                    }

                    return trimmed;
                }

                public static string WordWrap(string text, string prefix = "")
                {
                    string processed = "";
                    string temp = "";
                    int maxLength = MaxLength(prefix);

                    if (text.Length > maxLength)
                    {
                        string[] lines = Split(text);

                        foreach(string line in lines)
                        {
                            if (Color.CleanTags(line).Length < maxLength)
                            {
                                processed = processed.AppendLine(line);
                            }
                            else
                            {
                                var elementList = new List<string>();

                                if (Color.ContainsTag(line))
                                {
                                    foreach (string colorLine in Color.Expand(line))
                                    {
                                        foreach (Match match in RegexWordWrap.Matches(colorLine))
                                        {
                                            elementList.Add(match.Value);
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (Match match in RegexWordWrap.Matches(line))
                                    {
                                        elementList.Add(match.Value);
                                    }
                                }

                                foreach (string element in elementList)
                                {
                                    if (Color.CleanTags(temp + element).Length < maxLength)
                                    {
                                        temp += element;                                        
                                    }
                                    else
                                    {
                                        if (temp.EndsWith(' ') && temp.Length > maxLength - 10)
                                        {
                                            processed = processed.AppendLine(temp);
                                            temp = element;
                                        }
                                        else
                                        {
                                            temp += element;

                                            while (temp.Length > maxLength)
                                            {
                                                int length = maxLength + temp.Length - Color.CleanTags(temp).Length;
                                                string substring = temp.Substring(0, length);
                                                processed = processed.AppendLine(substring);
                                                temp = temp.Substring(length);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        return processed.AppendLine(temp);
                    }

                    return text;
                }
            }
        }
    }
}
