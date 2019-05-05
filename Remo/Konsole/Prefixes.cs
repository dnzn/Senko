namespace Konsole
{
    using System;
    using System.Text.RegularExpressions;
    using Global;

    public partial class Konsole
    {
        public Prefixes Prefix { get; private set; }

        public class Prefixes
        {
            public enum Setting
            {
                None,
                Prompt,
                Indent,
                Auto
            }

            Konsole This { get; set; }
            public bool Auto { get; set; } = true;

            Setting _current = Setting.Auto;
            Setting _actual = Setting.Prompt;
            Setting _previous = Setting.Prompt;
            public Setting Current
            {
                get
                {
                    return (_current == Setting.Auto) ? _actual : _current;
                }
                set
                {
                    if (value == Setting.Auto)
                    {
                        Auto = true;
                        _current = Setting.Auto;
                        _previous = _actual;
                        _actual = Setting.Prompt;
                        This.NewLine.Write = NewLines.Setting.Prepend;
                    }
                    else
                    {
                        Auto = false;
                        _current = value;
                        _previous = _actual;
                        _actual = value;

                        if (value != Setting.None)
                        {
                            This.NewLine.Write = NewLines.Setting.Prepend;
                        }
                    }
                }
            }

            Setting _override = Setting.Auto;
            public Setting Override
            {
                get
                {
                    Setting _temp = _override;
                    _override = Setting.Auto;
                    return _temp;
                }
                set
                {
                    _override = value;

                    if (!value.Is(Setting.Auto, Setting.None))
                    {
                        This.NewLine.OverrideWrite = NewLines.Setting.Prepend;
                    }
                }
            }

            public string Prompt { get; set; } = "KON > ";
            public string Indent { get { return new string(' ', Prompt.Length); } }

            public Prefixes(Konsole instance)
            {
                This = instance;
            }

            public Setting GetEmbeddedPrefix(string text)
            {
                if (text.Contains("prompt".Encapsulate("<") + Prompt))
                {
                    return Setting.Prompt;
                }
                else if (text.Contains("prompt".Encapsulate("<") + Indent))
                {
                    return Setting.Indent;
                }
                else
                {
                    return Setting.None;
                }
            }

            public string RemovePrefix(string text)
            {
                return Regex.Replace(text, @"<prompt>({0}|{1})".Format(Prompt, Indent), ""); //text.Replace("prompt".Encapsulate("<") + Prompt, "").Replace("prompt".Encapsulate("<") + Indent, "");
            }

            public string Insert(string text, Setting? setting = null)
            {
                if (setting == null)
                {
                    setting = Setting.Auto;
                }

                if (setting == Setting.Auto)
                {
                    setting = Current;
                }

                text = RemovePrefix(text);

                switch (setting)
                {
                    case Setting.Prompt:
                        text = Regex.Replace("<prompt>" + Prompt + "</>" + text, @"[\r\n]+", Environment.NewLine + Indent + "</>");
                        break;
                    case Setting.Indent:
                        text = Regex.Replace("<prompt>" + Indent + "</>" + text, @"[\r\n]+", Environment.NewLine + Indent + "</>");
                        break;
                }

                return text;
            }
        }
    }
}
