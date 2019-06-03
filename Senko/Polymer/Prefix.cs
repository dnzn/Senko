namespace Polymer
{
    using System;
    using System.Text.RegularExpressions;
    using static Konsole.Parameters;

    public static partial class Polymer
    {
        public static Regex RegexApplyIndentToAllLines { get; } = new Regex(Environment.NewLine + "|^", RegexOptions.Compiled);
        public static Regex RegexApplyIndentToNewLinesOnly { get; } = new Regex(Environment.NewLine, RegexOptions.Compiled); 
        public static Regex RegexContainsNonWhitespace { get; } = new Regex(@"[^ ]", RegexOptions.Compiled);

        public static string ApplyIndent(this string text, string indent, bool applyToAllLines = true)
        {
            Regex regex = (applyToAllLines) ? RegexApplyIndentToAllLines : RegexApplyIndentToNewLinesOnly;

            if (RegexContainsNonWhitespace.Match(text).Success)
            {
                return ApplyIndent(text, indent.Length, applyToAllLines);
            }
            else
            {
                return regex.Replace(text, @"$&" + indent);
            }
        }

        public static string ApplyIndent(this string text, int indentLength, bool applyToAllLines = true)
        {
            Regex regex = (applyToAllLines) ? RegexApplyIndentToAllLines : RegexApplyIndentToNewLinesOnly;

            var indent = new string(' ', indentLength);
            return regex.Replace(text, @"$&" + indent);
        }
    }

    public partial class Konsole
    {
        public enum PrefixType
        {
            Normal,
            Prompt,
            Indent,
            Auto,
            None
        }

        public Prefix Prefix { get; private set; }

        public partial class Parameters
        {
            public class Prefix
            {
                Konsole Parent { get; set; }

                bool _auto = true;
                public bool AutoPrefix
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
                public PrefixType CurrentPrefix
                {
                    get
                    {
                        return _current;
                    }
                    set
                    {
                        if (value == PrefixType.Auto)
                        {
                            AutoPrefix = true;
                        }
                        else
                        {
                            _current = value;

                            if (value != PrefixType.Normal)
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

                        if (!value.Is(PrefixType.Auto, PrefixType.Normal))
                        {
                            Parent.NewLine.OverrideWrite = NewLineType.Prepend;
                        }
                    }
                }

                public string Prompt { get; set; } = " KON> ";
                public string Indent { get { return new string(' ', Prompt.Length); } }
                public string Normal { get; set; } = " ";

                public Prefix(Konsole parent)
                {
                    Parent = parent;
                }

                public string RemovePrefix(string text)
                {
                    return Regex.Replace(text, @"^({0}|{1})".Format(GeneratePrefix(PrefixType.Prompt), GeneratePrefix(PrefixType.Indent)), "");
                }

                public string GeneratePrefix(PrefixType? prefixType, string text = null)
                {
                    string prefix = "";
                    
                    if (prefixType.IsNull() || prefixType == PrefixType.Auto)
                    {
                        prefixType = CurrentPrefix;
                    }

                    switch (prefixType)
                    {
                        case PrefixType.Prompt:
                            prefix = Color.CreateTag("prompt");
                            text = (text != null) ? "</>" + text.ApplyIndent(Indent, false) : "";
                            break;
                        case PrefixType.Indent:
                            prefix = Color.CreateTag("indent");
                            text = (text != null) ? text.ApplyIndent(Indent, false) : "";
                            break;
                        case PrefixType.Normal:
                            prefix = prefix.ApplyIndent(Normal);
                            text = (text != null) ? text.ApplyIndent(Normal) : "";
                            break;
                    }

                    return prefix + GetPrefix(prefixType) + text;
                }

                public string Insert(string text, PrefixType? prefixType = null)
                {
                    return GeneratePrefix(prefixType, NewLine.Standardize(RemovePrefix(text))); ;
                }

                public string GetPrefix(PrefixType? prefixType)
                {
                    string prefix = "";

                    switch (prefixType)
                    {
                        case PrefixType.Normal:
                            prefix = Normal;
                            break;
                        case PrefixType.Prompt:
                            prefix = Prompt;
                            break;
                        case PrefixType.Indent:
                            prefix = Indent;
                            break;
                        case PrefixType.Auto:
                            goto case PrefixType.Prompt;
                    }

                    return prefix;
                }
            }
        }
    }
}
