using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using static Remo.Global;

namespace Remo
{
    public class Konsole
    {
        //public enum KonsoleWrite { Inline, Newline }

        //static public bool VerboseMode = true;
        //static public bool IgnoreColor = false;
        //static public Prefix ForcePrefix = Prefix.Auto;
        //static public Prefix LastPrefix = Prefix.Prompt;
        //static public KonsoleWrite LastWrite = KonsoleWrite.Newline;      
        //static public string Prompt = "REMO> ";
        //static public ConsoleColor FormerColor;
        //static public string GlobalLog;

        public enum Prefix { None, Prompt, Indent, Auto }
        public enum NewLine { None, Before, After, Both }
        public enum Color { Primary, Secondary }

        // STATIC VARIABLES
        static public string KonsoleLog;
        static public bool VerboseMode = true;
        static public bool IgnoreColor = false;
        static public Prefix ForcePrefix = Prefix.Auto;
        static public Prefix? StaticLastPrefix = null;
        static public ConsoleColor? ForceColor = null;
        static public string Prompt = "KON> ";
        public static int Operations = 0;

        public ConsoleColor PrimaryColor { get; set; }
        public ConsoleColor SecondaryColor { get; set; }
        public ConsoleColor PromptColor { get; set; }
        public string Log { get; private set; }
        public bool LogOnly { get; set; }
        public Color CurrentColor { get; set; }
        public Prefix LastPrefix { get; private set; }
        public ConsoleColor LastColor { get; private set; }

        public Konsole()
        {
            LastPrefix = Prefix.Prompt;
            PrimaryColor = Console.ForegroundColor;
            SecondaryColor = Console.ForegroundColor;
            PromptColor = Console.ForegroundColor;
        }

        public void Write(bool primitive, ConsoleColor? color, NewLine newline, Prefix prefix, string text, params object[] args)
        {
            LastColor = Console.ForegroundColor;
            string console = "";
            string log = "";
            string nl = "";

            if (color == null) { color = (CurrentColor == Color.Primary) ? PrimaryColor : SecondaryColor;  }

            if (!IgnoreColor)
            {
                if (ForceColor != null) { color =  ForceColor; }

                Console.ForegroundColor = (ConsoleColor)color;
            }

            if (!primitive)
            {
                Operations++;
                log = DateTime.Now.ToString(Operations.ToString("D4").Encapsulate("[") + " MM/dd/yy HH:mm:ss | ");

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

                    LastPrefix = prefix;
                    StaticLastPrefix = prefix;
                }

                if (newline.Is(NewLine.Before, NewLine.Both))
                {
                    nl += Environment.NewLine;
                }

                if (Console.CursorLeft != 0)
                {
                    nl += Environment.NewLine;
                }

                if (prefix == Prefix.None)
                {
                    console += nl;
                }
                else
                {
                    text = Regex.Replace(text, @"(\n|\r\n?)", Environment.NewLine + new string(' ', Prompt.Length));
                }

                if (prefix == Prefix.Prompt)
                {
                    if (PromptColor == color)
                    {
                        console += nl + Prompt;
                    }
                    else
                    {
                        Write(true, (ConsoleColor)PromptColor, NewLine.None, Prefix.None, nl + Prompt);
                    }
                }
                else if (prefix == Prefix.Indent)
                {
                    console += (Console.CursorLeft == 0) ? nl + new string(' ', Prompt.Length) : console += " "; ;
                }
            }

            text = text.Replace(args);
            console += text;

            if (!primitive)
            {
                log += Regex.Replace(text, @"(\s{2,})", " ") + Environment.NewLine; ;
                Log += log;
                KonsoleLog += log;
            }

            if (newline.Is(NewLine.After, NewLine.Both))
            {
                console += Environment.NewLine;
            }

            if (VerboseMode)
            {
                Console.Write(console);
            }

            if (LastColor != Console.ForegroundColor)
            {
                Console.ForegroundColor = LastColor;

            }
        }

        // Konsole.Write Overrides

        public void Write(bool primitive, ConsoleColor? color, NewLine newline, string text, params object[] args)
        {
            Write(primitive, color, newline, Prefix.Auto, text, args);
        }

        public void Write(bool primitive, ConsoleColor? color, string text, params object[] args)
        {
            Write(primitive, color, NewLine.None, text, args);
        }

        public void Write(ConsoleColor? color, NewLine newline, string text, params object[] args)
        {
            Write(false, color, newline, text, args);
        }

        public void Write(bool primitive, NewLine newline, string text, params object[] args)
        {
            Write(primitive, null, newline, Prefix.Auto, text, args);
        }

        public void Write(NewLine newline, string text, params object[] args)
        {
            Write(false, newline, text, args);
        }

        public void Write(bool primitive, ConsoleColor? color, Prefix prefix, string text, params object[] args)
        {
            Write(primitive, color, NewLine.None, prefix, text, args);
        }

        public void Write(ConsoleColor? color, Prefix prefix, string text, params object[] args)
        {
            Write(false, color, prefix, text, args);
        }

        public void Write(ConsoleColor? color, string text, params object[] args)
        {
            Write(color, Prefix.Auto, text, args);
        }

        public void Write(bool primitive, NewLine newline, Prefix prefix, string text, params object[] args)
        {
            Write(primitive, null, newline, prefix, text, args);
        }

        public void Write(bool primitive, Prefix prefix, string text, params object[] args)
        {
            Write(primitive, NewLine.None, prefix, text, args);
        }

        public void Write(bool primitive, string text, params object[] args)
        {
            Write(primitive, Prefix.Auto, text, args);
        }

        public void Write(ConsoleColor? color, NewLine newline, Prefix prefix, string text, params object[] args)
        {
            Write(false, color, newline, prefix, text, args);
        }

        public void Write(NewLine newline, Prefix prefix, string text, params object[] args)
        {
            Write(null, newline, prefix, text, args);
        }

        public void Write(Prefix prefix, string text, params object[] args)
        {
            Write(NewLine.None, prefix, text, args);
        }

        public void Write(string text, params object[] args)
        {
            Write(Prefix.Auto, text, args);
        }

        // Konsole.WriteLine()

        public void WriteLine(ConsoleColor? color, NewLine newline, Prefix prefix, string text, params object[] args)
        {
            switch (newline)
            {
                case NewLine.None:
                    newline = NewLine.After;
                    break;
                case NewLine.Before:
                    newline = NewLine.Both;
                    break;
                default:
                    break;
            }

            Write(color, newline, prefix, text, args);
        }

        public void WriteLine(ConsoleColor? color, NewLine newline, string text, params object[] args)
        {
            WriteLine(color, newline, text, Prefix.Auto, args);
        }
        
        public void WriteLine(NewLine newline, string text, params object[] args)
        {
            WriteLine(null, newline, text, args);
        }

        public void WriteLine(NewLine newline, Prefix prefix, string text, params object[] args)
        {
            WriteLine(null, newline, prefix, text, args);
        }

        public void WriteLine(Prefix prefix, string text, params object[] args)
        {
            WriteLine(NewLine.After, prefix, text, args);
        }

        public void WriteLine(string text, params object[] args)
        {
            WriteLine(Prefix.Auto, text, args);
        }
    }
}
