using System;
using System.Collections.Generic;
using System.Text;

namespace Remo
{
    public partial class Konsole
    {
        public Newline NewLine { get; private set; }

        public class Newline
        {
            public enum Setting
            {
                None,
                Before,
                After,
                Both
            }

            Konsole _kon { get; set; }
            public Setting Write { get; set; } = Setting.None;

            public Setting WriteLine
            {
                get
                {
                    switch (Write)
                    {
                        case Setting.None:
                            return Setting.After;
                        case Setting.Before:
                            return Setting.Both;
                        default:
                            return Write;
                    }
                }
            }

            public Newline(Konsole instance)
            {
                _kon = instance;
            }

            public string Insert (string text, bool ignoreSetting, bool writeline = false)
            {
                Setting setting;

                switch (writeline)
                {
                    case true:
                        setting = WriteLine;
                        break;
                    default:
                        setting = Write;
                        break;
                }

                if (setting == Setting.Before || setting == Setting.Both)
                {
                    text = Environment.NewLine + text;
                }

                if (setting == Setting.After || setting == Setting.Both)
                {
                    text = text + Environment.NewLine;
                }

                return text;
            }
        }
    }
}
