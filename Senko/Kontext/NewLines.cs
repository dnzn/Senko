namespace Kontext
{
    using System;
    using System.Text.RegularExpressions;

    public partial class Konsole
    {
        public enum NewLineType
        {
            None,
            Prepend,
            Append,
            Both
        }

        public Parameters.NewLine NewLine { get; private set; }
        
        public partial class Parameters
        {
            public class NewLine
            {
                Konsole This { get; set; }
                public NewLineType Former { get; set; } = NewLineType.None;

                NewLineType _write { get; set; } = NewLineType.None;
                public NewLineType Write
                {
                    get { return _write; }
                    set
                    {
                        Former = Write;
                        _write = value;
                    }
                }

                public NewLineType WriteLine
                {
                    get { return ToWriteLine(Write); }
                    set { Write = value; }
                }

                NewLineType _override { get; set; } = NewLineType.None;
                public NewLineType OverrideWrite
                {
                    get
                    {
                        NewLineType _temp = _override;
                        _override = Write;
                        return _temp;
                    }
                    set { _override = value; }
                }

                public NewLineType OverrideWriteLine
                {
                    get { return ToWriteLine(OverrideWrite); }
                    set { _override = value; }
                }

                public NewLine(Konsole instance)
                {
                    This = instance;
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

                    text = ((setting == NewLineType.Prepend || setting == NewLineType.Both) && CursorPosition != 0) ? Environment.NewLine + text : text;
                    text = (setting == NewLineType.Append || setting == NewLineType.Both) ? text + Environment.NewLine : text;

                    return text;
                }

                public static string Flush(string text)
                {
                    return Regex.Replace(text, @"[\r\n]+", Environment.NewLine);
                }
            }
        }
    }
}
