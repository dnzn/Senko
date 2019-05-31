namespace Polymer
{
    using System;
    using System.Text.RegularExpressions;
    using Generic;

    using static Konsole.Parameters;

    public partial class Konsole
    {
        public enum PrefixType
        {
            None,
            Prompt,
            Indent,
            Auto
        }

        public Prefix Prefix { get; private set; }

        public partial class Parameters
        {
            public class Prefix
            {
                Konsole Parent { get; set; }

                bool _auto = true;
                public bool Auto
                {
                    get { return _auto; }
                    set
                    {
                        if (value)
                        {
                            _current = PrefixType.Prompt;
                            Parent.NewLine.Write = NewLineType.Prepend;
                        }

                        _auto = value;
                    }
                }

                PrefixType _current = PrefixType.Prompt;
                public PrefixType Current
                {
                    get
                    {
                        return _current;
                    }
                    set
                    {
                        if (value == PrefixType.Auto)
                        {
                            Auto = true;
                        }
                        else
                        {
                            _current = value;

                            if (value != PrefixType.None)
                            {
                                Parent.NewLine.Write = NewLineType.Prepend;
                            }
                        }
                    }
                }

                PrefixType _override = PrefixType.Auto;
                public PrefixType Override
                {
                    get
                    {
                        PrefixType _temp = _override;
                        _override = PrefixType.Auto;
                        return _temp;
                    }
                    set
                    {
                        _override = value;

                        if (!value.Is(PrefixType.Auto, PrefixType.None))
                        {
                            Parent.NewLine.OverrideWrite = NewLineType.Prepend;
                        }
                    }
                }

                public string Prompt { get; set; } = " KON> ";
                public string Indent { get { return new string(' ', Prompt.Length); } }

                public Prefix(Konsole parent)
                {
                    Parent = parent;
                }

                public string RemovePrefix(string text)
                {
                    return Regex.Replace(text, @"^({0}|{1})".Format(Color.CreateTag("prompt") + Prompt, Color.CreateTag("indent") + Indent), "");
                }

                public string Insert(string text, PrefixType? setting = null)
                {
                    if (setting == null)
                    {
                        setting = PrefixType.Auto;
                    }

                    if (setting == PrefixType.Auto)
                    {
                        setting = Current;
                    }

                    text = RemovePrefix(text);
                    text = NewLine.Flush(text);

                    switch (setting)
                    {
                        case PrefixType.Prompt:
                            text = IndentNewLines(Color.CreateTag("prompt") + Prompt + "</>" + text);
                            break;
                        case PrefixType.Indent:
                            text = IndentNewLines(Color.CreateTag("indent") + Indent + text);
                            break;
                    }

                    return text;
                }

                string IndentNewLines(string text)
                {
                    return text.Replace(Environment.NewLine, Environment.NewLine + Indent);
                }

                public string GetPrefix(PrefixType prefixType)
                {
                    switch (prefixType)
                    {
                        case PrefixType.None:
                            return "";
                        case PrefixType.Prompt:
                            return Prompt;
                        case PrefixType.Indent:
                            return Indent;
                        default:
                            goto case PrefixType.Prompt;
                    }
                }
            }
        }
    }
}
