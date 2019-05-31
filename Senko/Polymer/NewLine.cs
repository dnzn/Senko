namespace Polymer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Generic;

    using static Kontext;

    public partial class Konsole
    {
        public enum NewLineType
        {
            None,
            Prepend,
            Append,
            Both
        }

        public enum WordWrap
        {
            Enabled,
            Disabled
        }

        public Parameters.NewLine NewLine { get; private set; }

        public partial class Parameters
        {
            public class NewLine
            {
                static Regex RegexFlush { get; } = new Regex(@"[\r\n]+");
                static Regex RegexWordWrap { get; } = new Regex(@"<\w+>|\S+\s*");

                Konsole Parent { get; set; }
                public NewLineType Former { get; set; } = NewLineType.None;

                NewLineType _Write { get; set; } = NewLineType.None;
                public NewLineType Write
                {
                    get { return _Write; }
                    set
                    {
                        Former = Write;
                        _Write = value;
                    }
                }

                public NewLineType WriteLine
                {
                    get { return ToWriteLine(Write); }
                    set { Write = value; }
                }

                NewLineType _Override { get; set; } = NewLineType.None;
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
                    get { return ToWriteLine(OverrideWrite); }
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
                /// <param name="setting">The setting the apply</param>
                /// <returns>The modified text</returns>
                public string Insert(string text, NewLineType? setting = null)
                {
                    if (setting == null)
                    {
                        setting = Write;
                    }

                    text = ((setting == NewLineType.Prepend || setting == NewLineType.Both) && CursorLeft != 0) ? Environment.NewLine + text : text;
                    text = (setting == NewLineType.Append || setting == NewLineType.Both) ? text + Environment.NewLine : text;

                    return text;
                }

                public static string Flush(string text)
                {
                    return RegexFlush.Replace(text, Environment.NewLine);
                }

                public static string[] Split(string text)
                {
                    text = Flush(text);

                    return text.Split(Environment.NewLine);
                }

                public static string WordWrap(string text, Konsole konsole = null)
                {
                    konsole = konsole ?? MainKonsole;

                    return WordWrap(text, konsole.Prefix.Current, konsole.Prefix.Prompt);
                }

                public static string WordWrap(string text, PrefixType prefixType, string prefix = "")
                {
                    string final = "";
                    int maxLength = WindowWidth - 1 - ((prefixType != PrefixType.None) ? prefix.Length : 1);
                    string temp = "";

                    if (text.Length > maxLength)
                    {
                        MatchCollection matches = RegexWordWrap.Matches(text);

                        foreach (Match match in matches)
                        {
                            if (match.Value.Length > maxLength)
                            {
                                for (int i = 0; i < match.Value.Length; i += maxLength)
                                {
                                    temp = match.Value.Substring(i, Math.Min(maxLength, match.Value.Length - i));
                                    final = final.AppendLine(temp);
                                    temp = "";
                                }
                            }
                            else if (Color.CleanTags(temp + match.Value).Length < maxLength && !match.Value.Contains(Environment.NewLine))
                            {
                                temp += match.Value;
                            }
                            else if (match.Value.Contains(Environment.NewLine))
                            {
                                final += temp;
                                temp = match.Value;
                            }
                            else
                            {
                                final = final.AppendLine(temp);
                                temp = match.Value;
                            }
                        }

                        final = final.AppendLine(temp);
                    }
                    else { final = text; }
                                       
                    return final;
                }
            }
        }
    }
}
