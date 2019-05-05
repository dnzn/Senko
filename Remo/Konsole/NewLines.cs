namespace Konsole
{
    using System;

    public partial class Konsole
    {
        public NewLines NewLine { get; private set; }

        public class NewLines
        {
            public enum Setting
            {
                None,
                Prepend,
                Append,
                Both
            }

            Konsole This { get; set; }
            public Setting Former { get; set; } = Setting.None;

            Setting _write { get; set; } = Setting.None;
            public Setting Write
            {
                get { return _write; }
                set
                {
                    Former = Write;
                    _write = value;
                }
            }

            public Setting WriteLine
            {
                get { return ToWriteLine(Write); }
                set { Write = value; }
            }

            Setting _override { get; set; } = Setting.None;
            public Setting OverrideWrite
            {
                get
                {
                    Setting _temp = _override;
                    _override = Write;
                    return _temp;
                }
                set { _override = value; }
            }

            public Setting OverrideWriteLine
            {
                get { return ToWriteLine(OverrideWrite); }
                set { _override = value; }
            }

            public NewLines(Konsole instance)
            {
                This = instance;
            }

            Setting ToWriteLine(Setting setting)
            {
                switch (setting)
                {
                    case Setting.None:
                        return Setting.Append;
                    case Setting.Prepend:
                        return Setting.Both;
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
            public string Insert(string text, Setting? setting = null)
            {
                if (setting == null)
                {
                    setting = Write;
                }

                text = (setting == Setting.Prepend || setting == Setting.Both) ? Environment.NewLine + text : text;
                text = (setting == Setting.Append || setting == Setting.Both) ? text + Environment.NewLine : text;

                return text;
            }
        }
    }
}
