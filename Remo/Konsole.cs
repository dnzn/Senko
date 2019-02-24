using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using static Remo.Global;

namespace Remo
{
    public class Konsole
    {
        public enum Prefix { None, Prompt, Indent, Auto }
        public enum NewLine { None, Above, Below, Both }
        public enum KonsoleWrite { Inline, Newline }

        static public bool VerboseMode = true;
        static public bool IgnoreColor = false;
        static public Prefix ForcePrefix = Prefix.Auto;
        static public Prefix LastPrefix = Prefix.Prompt;
        static public KonsoleWrite LastWrite = KonsoleWrite.Newline;      
        static public string Prompt = "REMO> ";
        static public ConsoleColor FormerColor;
        static public string Log;

        public bool LogOnly = false;

        public ConsoleColor Color { get; set; }
        public ConsoleColor PromptColor { get; set; }
                
        public Konsole(ConsoleColor color, ConsoleColor promptcolor, string prompt)
        {
            Init(color, promptcolor, prompt);
        }

        public Konsole(ConsoleColor color, ConsoleColor promptcolor)
        {
            Init(color, color);
        }

        public Konsole(ConsoleColor color, string prompt)
        {
            Init(color, prompt);
        }

        public Konsole(ConsoleColor color)
        {
            Init(color, GetCurrentColor(), Prompt);
        }

        public Konsole(string prompt)
        {
            Init(prompt);
        }

        public Konsole(bool logonly)
        {
            Init(logonly);
        }

        public Konsole()
        {
            Init();
        }

        void Init(ConsoleColor color, ConsoleColor promptcolor, string prompt)
        {
            Init(color, promptcolor);
            Prompt = prompt;
        }

        void Init(ConsoleColor color, ConsoleColor promptcolor)
        {
            Init(color);
            PromptColor = promptcolor;
        }

        void Init(ConsoleColor color, string prompt)
        {
            Init(color);
            Prompt = prompt;
        }

        void Init(ConsoleColor color)
        {
            Init();
            Color = color;
        }

        void Init(string prompt)
        {
            Init();
            Prompt = prompt;
        }

        void Init(bool logonly)
        {
            if (logonly)
            {
                LogOnly = logonly;
            }

            Init();
        }

        void Init()
        {
            Console.CursorVisible = false;
            Console.ResetColor();
            Color = GetCurrentColor();
            PromptColor = GetCurrentColor();

            SaveFormerColor();
        }

        // Write methods and overrides

        public void Write(ConsoleColor? tempcolor, NewLine newline, Prefix prefix, object obj, params object[] args)
        {
            DateTime datetime = DateTime.Now;
            string datetimeString = datetime.ToString("MM/dd/yy HH:mm:ss | ");

            string consoleString = "";
            string oldString = (obj == null) ? "" : obj.ToString();

            ToggleColor(tempcolor);

            if (ForcePrefix != Prefix.Auto)
            {
                prefix = ForcePrefix;
            }
            else
            {
                if (prefix == Prefix.Auto)
                {
                    prefix = LastPrefix;
                }
                else
                {
                    LastPrefix = prefix;
                }
            }

            if (prefix == Prefix.Prompt)
            {
                Log += Environment.NewLine + datetimeString;

                if (VerboseMode && (LastWrite == KonsoleWrite.Newline || newline.Is(NewLine.Above, NewLine.Both)))
                {

                    ConsoleColor fcolor = FormerColor;
                    Write(PromptColor, NewLine.None, Prefix.None, Prompt);
                    FormerColor = fcolor;
                }
            }
            else if (prefix == Prefix.Indent)
            {
                Log += Environment.NewLine + datetimeString;

                if (LastWrite == KonsoleWrite.Newline || newline.Is(NewLine.Above, NewLine.Both))
                {
                    consoleString += new string(' ', Prompt.Length);
                }
                else
                {
                    consoleString = " ";
                }
            }

            oldString = oldString.Replace(args);

            consoleString += Regex.Replace(oldString, @"(\n|\r\n?)", Environment.NewLine + new string(' ', Prompt.Length));

            if (newline.Is(NewLine.Above, NewLine.Both))
            {
                consoleString = Environment.NewLine + consoleString;
            }

            if (newline.Is(NewLine.Below, NewLine.Both))
            {
                consoleString += Environment.NewLine;
            }

            LastWrite = newline.Is(NewLine.Above, NewLine.None) ? KonsoleWrite.Inline : KonsoleWrite.Newline;

            if (VerboseMode && !LogOnly)
            {
                Console.Write(consoleString);
            }

            ToggleColor(tempcolor);

            Log += oldString != "REMO> " ? Regex.Replace(Regex.Replace(oldString, @"(\n|\r\n?)", " "), @"(\s{2,})", " ") : "";
        }

        public void Write(NewLine newline, Prefix prefix, object obj, params object[] args)
        {
            Write(null, newline, prefix, obj, args);
        }

        public void Write(Prefix prefix, object obj, params object[] args)
        {
            Write(NewLine.None, prefix, obj, args);
        }

        public void Write(object obj, params object[] args)
        {
            Write(NewLine.None, Prefix.Auto, obj, args);
        } 

        public void WriteLine(Prefix prefix, object obj, params object[] args)
        {
            Write(NewLine.Below, prefix, obj, args);
        }

        public void WriteLine(object obj, params object[] args)
        {
            Write(NewLine.Below, Prefix.Auto, obj, args);
        }

        public void WriteLine()
        {
            Prefix formerPrefix = LastPrefix;

            Write(NewLine.Below, Prefix.None, null);

            LastPrefix = formerPrefix;
        }

        public void ColorWrite(ConsoleColor tempcolor, Prefix prefix, object obj, params object[] args)
        {
            Write(tempcolor, NewLine.None, prefix, obj, args);
        }

        public void ColorWrite(ConsoleColor tempcolor, object obj, params object[] args)
        {
            Write(tempcolor, NewLine.None, Prefix.Auto, obj, args);
        }

        public void ColorWriteLine(ConsoleColor tempcolor, Prefix prefix, object obj, params object[] args)
        {
            Write(tempcolor, NewLine.Below, prefix, obj, args);
        }

        public void ColorWriteLine(ConsoleColor tempcolor, object obj, params object[] args)
        {
            Write(tempcolor, NewLine.Below, Prefix.Auto, obj, args);
        }

        // Konsole color methods

        ConsoleColor GetCurrentColor()
        {
            return Console.ForegroundColor;
        }

        void SaveFormerColor()
        {
            FormerColor = GetCurrentColor();
        }

        void ToggleColor(ConsoleColor? color = null)
        {
            if (!IgnoreColor)
            {
                ConsoleColor setcolor = color ?? Color;

                if (Console.ForegroundColor != setcolor)
                {
                    SaveFormerColor();
                    Console.ForegroundColor = setcolor;
                }
                else
                {
                    if (setcolor != FormerColor)
                    {
                        Console.ForegroundColor = FormerColor;
                    }
                }
            }
            else
            {
                Console.ResetColor();
            }
        }

        static public bool ToggleVerboseMode()
        {
            return VerboseMode = !VerboseMode;
        }

        static public bool ToggleIgnoreColor()
        {
            return IgnoreColor = !IgnoreColor;
        }

        static public void ResetForcePrefix()
        {
            ForcePrefix = Prefix.Auto;
        }
    }
}
