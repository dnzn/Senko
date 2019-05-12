namespace Kontext
{
    using System;
    using System.Text.RegularExpressions;
    using Global;

    public partial class Konsole
    {
        public enum PrefixType
        {
            None,
            Prompt,
            Indent,
            Auto
        }

        public Parameters.Prefix Prefix { get; private set; }

        public partial class Parameters
        {
            public class Prefix
            {
                Konsole Parent { get; set; }
                public bool Auto { get; set; } = true;

                PrefixType _current = PrefixType.Auto;
                PrefixType _actual = PrefixType.Prompt;
                PrefixType _previous = PrefixType.Prompt;
                public PrefixType Current
                {
                    get
                    {
                        return (_current == PrefixType.Auto) ? _actual : _current;
                    }
                    set
                    {
                        if (value == PrefixType.Auto)
                        {
                            Auto = true;
                            _current = PrefixType.Auto;
                            _previous = _actual;
                            _actual = PrefixType.Prompt;
                            Parent.NewLine.Write = NewLineType.Prepend;
                        }
                        else
                        {
                            Auto = false;
                            _current = value;
                            _previous = _actual;
                            _actual = value;

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

                public string Prompt { get; set; } = "KON> ";
                public string Indent { get { return new string(' ', Prompt.Length); } }

                public Prefix(Konsole parent)
                {
                    Parent = parent;
                }

                public PrefixType GetEmbeddedPrefix(string text)
                {
                    if (text.Contains("prompt".Encapsulate("<") + Prompt))
                    {
                        return PrefixType.Prompt;
                    }
                    else if (text.Contains("prompt".Encapsulate("<") + Indent))
                    {
                        return PrefixType.Indent;
                    }
                    else
                    {
                        return PrefixType.None;
                    }
                }

                public string RemovePrefix(string text)
                {
                    return Regex.Replace(text, @"<prompt>({0}|{1})".Format(Prompt, Indent), ""); //text.Replace("prompt".Encapsulate("<") + Prompt, "").Replace("prompt".Encapsulate("<") + Indent, "");
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
                    text = Parameters.NewLine.Flush(text);

                    switch (setting)
                    {
                        case PrefixType.Prompt:
                            text = Regex.Replace(FormatText(Prompt, text, "<prompt>"), @Environment.NewLine, FormatInsert(Indent, @"$&"));
                            break;
                        case PrefixType.Indent:
                            text = Regex.Replace(FormatText(Indent, text), @Environment.NewLine, FormatInsert(Indent, @"$&"));
                            break;
                    }

                    return text;
                }

                string FormatInsert(string prefix, string insert = "")
                {
                    return FormatText(prefix, "", insert);
                }

                string FormatText(string prefix, string text, string insert = "")
                {
                    return insert + prefix + "</>" + text;
                }
            }
        }
    }
}
