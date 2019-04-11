namespace Remo
{
    using System;
    using System.Text.RegularExpressions;

    public class Konsole
    {
        public enum Prefix
        { 
            None,
            Prompt,
            Indent,
            Auto
        }

        public enum NewLine
        { 
            None,
            Before,
            After,
            Both
        }

        public enum Color
        {
            Primary,
            Secondary
        }

        static public string Log;

        static public bool VerboseMode = true;

        static public bool IgnoreColor = false;

        static public Prefix ForcePrefix = Prefix.Auto;

        static public Prefix? LastPrefix = null;

        static public ConsoleColor? ForceColor = null;

        static public string Prompt = "KON> ";

        public static int Operations = 0;

        public ConsoleColor PrimaryColor { get; set; }

        public ConsoleColor SecondaryColor { get; set; }

        public ConsoleColor PromptColor { get; set; }

        public bool LogOnly { get; set; }

        public Color CurrentColor { get; set; }

        public string InstanceLog { get; private set; }

        internal Prefix lastPrefix { get; set; }

        internal ConsoleColor lastColor { get; set; }

        public Konsole()
        {
            lastPrefix = Prefix.Prompt;
            PrimaryColor = Console.ForegroundColor;
            SecondaryColor = Console.ForegroundColor;
            PromptColor = Console.ForegroundColor;
            Console.CursorVisible = false;
            InstanceLog = "";
            Log = "";
        }

        public int Read()
        {
            WritePrompt();
            Console.CursorVisible = true;
            int read = Console.Read();
            Console.CursorVisible = false;
            return read;
        }

        public string ReadLine()
        {
            Operations++;
            string log = DateTime.Now.ToString(Operations.ToString("D4").Encapsulate("[") + " MM/dd/yy HH:mm:ss < ");

            WritePrompt();
            Console.CursorVisible = true;
            string read = Console.ReadLine();
            Console.CursorVisible = false;

            log += read;
            InstanceLog += InstanceLog.Is("", null) ? log : Environment.NewLine + log;
            Log += Log.Is("", null) ? log : Environment.NewLine + log;

            return read;
        }

        public void Write(bool primitive, ConsoleColor? color, NewLine newline, Prefix prefix, string text, params object[] args)
        {
            lastColor = Console.ForegroundColor;
            string console = "";
            string log = "";
            string nl = "";
            bool disableLog = false;

            if (text.Is(Log, InstanceLog, ""))
            {
                disableLog = true;
            }

            if (!primitive)
            {
                Operations++;

                log = DateTime.Now.ToString(Operations.ToString("D4").Encapsulate("[") + " MM/dd/yy HH:mm:ss > ");

                if (ForcePrefix != Prefix.Auto)
                {
                    prefix = ForcePrefix;
                }
                else
                {
                    if (prefix == Prefix.Auto)
                    {
                        prefix = lastPrefix;
                    }

                    lastPrefix = prefix;
                    LastPrefix = prefix;
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
                    if (text != null)
                    {
                        text = Regex.Replace(text, @"(\n|\r\n?)", Environment.NewLine + new string(' ', Prompt.Length), RegexOptions.Multiline);
                    }
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

            if (color == null)
            {
                color = (CurrentColor == Color.Primary) ? PrimaryColor : SecondaryColor;
            }
            else
            {
                if (color == PrimaryColor)
                {
                    CurrentColor = Color.Primary;
                }
                else
                {
                    SecondaryColor = (ConsoleColor)color;
                }
            }

            if (!IgnoreColor)
            {
                if (ForceColor != null) { color = ForceColor; }

                Console.ForegroundColor = (ConsoleColor)color;
            }

            text = text.Format(args);
            console += text;

            if (!primitive && !disableLog)
            {
                log += text; //Regex.Replace(text, @"(\s{2,})", " ", RegexOptions.Multiline);

                InstanceLog += InstanceLog.Is("", null) ? log : Environment.NewLine + log;
                Log += Log.Is("", null) ? log : Environment.NewLine + log;
            }

            if (newline.Is(NewLine.After, NewLine.Both))
            {
                console += Environment.NewLine;
            }

            if (VerboseMode)
            {
                Console.Write(console);
            }

            if (lastColor != Console.ForegroundColor)
            {
                Console.ForegroundColor = lastColor;
            }

            Console.ResetColor();
        }

        public void Write(bool primitive, ConsoleColor? color, NewLine newline, string text, params object[] args)
        {
            Write(primitive, color, newline, Prefix.Auto, text, args);
        }

        public void Write(bool primitive, ConsoleColor? color, Prefix prefix, string text, params object[] args)
        {
            Write(primitive, color, NewLine.None, prefix, text, args);
        }

        public void Write(bool primitive, NewLine newline, Prefix prefix, string text, params object[] args)
        {
            Write(primitive, null, newline, prefix, text, args);
        }

        public void Write(bool primitive, NewLine newline, string text, params object[] args)
        {
            Write(primitive, null, newline, text, args);
        }

        public void Write(bool primitive, Prefix prefix, string text, params object[] args)
        {
            Write(primitive, NewLine.None, prefix, text, args);
        }

        public void Write(bool primitive, ConsoleColor? color, string text, params object[] args)
        {
            Write(primitive, color, Prefix.Auto, text, args);
        }

        public void Write(bool primitive, string text, params object[] args)
        {
            Write(primitive, Prefix.Auto, text, args);
        }

        public void Write(ConsoleColor? color, NewLine newline, Prefix prefix, string text, params object[] args)
        {
            Write(false, color, newline, prefix, text, args);
        }

        public void Write(ConsoleColor? color, NewLine newline, string text, params object[] args)
        {
            Write(color, newline, Prefix.Auto, text, args);
        }

        public void Write(ConsoleColor? color, Prefix prefix, string text, params object[] args)
        {
            Write(color, NewLine.None, prefix, text, args);
        }

        public void Write(ConsoleColor? color, string text, params object[] args)
        {
            Write(color, Prefix.Auto, text, args);
        }

        public void Write(NewLine newline, Prefix prefix, string text, params object[] args)
        {
            Write(false, null, newline, prefix, text, args);
        }

        public void Write(NewLine newline, string text, params object[] args)
        {
            Write(newline, Prefix.Auto, text, args);
        }

        public void Write(Prefix prefix, string text, params object[] args)
        {
            Write(NewLine.None, prefix, text, args);
        }

        public void Write(string text, params object[] args)
        {
            Write(Prefix.Auto, text, args);
        }

        public void WritePrompt()
        {
            Write(Prefix.Prompt, "");
        }

        public void WriteLine(bool primitive, ConsoleColor? color, NewLine newline, Prefix prefix, string text, params object[] args)
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

            Write(primitive, color, newline, prefix, text, args);
        }

        public void WriteLine(bool primitive, ConsoleColor? color, NewLine newline, string text, params object[] args)
        {
            WriteLine(primitive, color, newline, Prefix.Auto, text, args);
        }

        public void WriteLine(bool primitive, ConsoleColor? color, Prefix prefix, string text, params object[] args)
        {
            WriteLine(primitive, color, NewLine.After, prefix, text, args);
        }

        public void WriteLine(bool primitive, NewLine newline, Prefix prefix, string text, params object[] args)
        {
            WriteLine(primitive, null, newline, prefix, text, args);
        }

        public void WriteLine(bool primitive, NewLine newline, string text, params object[] args)
        {
            WriteLine(primitive, null, newline, text, args);
        }

        public void WriteLine(bool primitive, Prefix prefix, string text, params object[] args)
        {
            WriteLine(primitive, NewLine.After, prefix, text, args);
        }

        public void WriteLine(bool primitive, ConsoleColor? color, string text, params object[] args)
        {
            WriteLine(primitive, color, Prefix.Auto, text, args);
        }

        public void WriteLine(bool primitive, string text, params object[] args)
        {
            WriteLine(primitive, Prefix.Auto, text, args);
        }

        public void WriteLine(ConsoleColor? color, NewLine newline, Prefix prefix, string text, params object[] args)
        {
            WriteLine(false, color, newline, prefix, text, args);
        }

        public void WriteLine(ConsoleColor? color, NewLine newline, string text, params object[] args)
        {
            WriteLine(color, newline, Prefix.Auto, text, args);
        }

        public void WriteLine(ConsoleColor? color, Prefix prefix, string text, params object[] args)
        {
            WriteLine(color, NewLine.After, prefix, text, args);
        }

        public void WriteLine(ConsoleColor? color, string text, params object[] args)
        {
            WriteLine(color, Prefix.Auto, text, args);
        }

        public void WriteLine(NewLine newline, Prefix prefix, string text, params object[] args)
        {
            WriteLine(false, null, newline, prefix, text, args);
        }

        public void WriteLine(NewLine newline, string text, params object[] args)
        {
            WriteLine(newline, Prefix.Auto, text, args);
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
