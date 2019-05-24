namespace Kontext
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

                public string RemovePrefix(string text)
                {
                    return Regex.Replace(text, @"^({0}|{1})".Format(Color.CreateTag("prompt") + Prompt, Indent), "");
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
                            text = IndentNewLines(Indent + "</>" + text);
                            break;
                    }

                    return text;
                }

                string IndentNewLines(string text)
                {
                    return text.Replace(Environment.NewLine, Environment.NewLine + Indent);
                }
            }
        }
    }
}
