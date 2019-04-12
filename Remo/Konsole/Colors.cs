namespace Remo
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Linq;

    public partial class Konsole
    {
        public Colors Color { get; private set; }
        
        public class Colors
        {
            Konsole _kon { get; set; }
            public ConsoleColor Primary { get; set; } = ConsoleColor.White;
            public ConsoleColor Secondary { get; set; } = ConsoleColor.Gray;
            public ConsoleColor Prompt { get; set; } = ConsoleColor.DarkGray;
            public ConsoleColor? Former { get; private set; }

            public ConsoleColor Current
            {
                get { return Console.ForegroundColor; }
                set
                {
                    if (Current != value)
                    {
                        ConsoleColor formerColor = Former ?? Current;

                        if (formerColor != Current)
                        {
                            Former = Console.ForegroundColor;
                        }

                        Console.ForegroundColor = value;
                    }
                }
            }

            public ConsoleColor Background
            {
                get { return Console.BackgroundColor; }
                set
                {
                    if (Background != value)
                    {
                        Console.BackgroundColor = value;
                        Console.Clear();
                    }
                }
            }

            public ConsoleColor Random
            {
                get
                {
                    List<ConsoleColor> colors = Enum.GetValues(typeof(ConsoleColor)).Cast<ConsoleColor>().ToList();
                    Random rnd = new Random();
                    int r = rnd.Next(colors.Count);

                    while (colors[r] == Current || colors[r] == Background)
                    {
                        r = rnd.Next(colors.Count);
                    }

                    return colors[r];
                }
            }

            public Colors(Konsole instance)
            {
                _kon = instance;
            }

            /// <summary>
            /// Toggles the Current color with the specified color
            /// </summary>
            /// <param name="color">The color to toggle to. It will set to Former if null.</param>
            public void Toggle(ConsoleColor? color = null)
            {
                Current = color ?? Former ?? Current;
            }

            /// <summary>
            /// Switch Current color between Primary and Secondary colors or set to Primary if Current is not any of the two.
            /// </summary>
            public void Switch()
            {
                if (Current == Primary)
                {
                    Current = Secondary;
                }
                else
                {
                    Current = Primary;
                }
            }

            /// <summary>
            /// Checks if ConsoleColor contains a color represented by a string. This is case-insensitive.
            /// </summary>
            /// <param name="color">The color to check.</param>
            /// <returns>True if color is found.</returns>
            public bool Contains(string color)
            {
                foreach (string consolecolor in Enum.GetNames(typeof(ConsoleColor)))
                {
                    if (color.ToLower() == consolecolor.ToLower())
                    {
                        return true;
                    }
                }

                return false;
            }

            public string Paint(string text)
            {
                string c = Regex.Match(text, @"<(\w+)>").Groups[1].Value;

                if (Contains(c))
                {
                    ConsoleColor color = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), c, true);

                    Current = color;
                }

                return Regex.Replace(text, @"<\w+>", "");
            }

            public string[] Split(string text)
            {
                text = Regex.Replace(text, @"<\w+>", @"</>$&");
                text = Regex.Replace(text, @"<\\(\w+>)", @"<$1");

                return text.Split("</>",StringSplitOptions.RemoveEmptyEntries);
            }
        }
    }
}
