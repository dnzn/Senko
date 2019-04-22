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
            public enum Palette { Rainbow, RainbowWave, Light, Dark, LightGradient, LightGradientWave, DarkGradient, DarkGradientWave, Random, RandomLight, RandomDark, All };

            static Dictionary<string, string[]> Palettes { get; } = new Dictionary<string, string[]>
            {
                { "Rainbow", new string[] { "Red", "Yellow", "Green", "Cyan", "Magenta" }  },
                { "Light", new string[] { "Red", "Yellow", "Green", "Cyan", "Magenta", "White", "Gray" } },
                { "Dark", new string[] { "Black", "DarkGray", "DarkCyan", "Blue", "DarkBlue", "DarkYellow", "DarkGreen", "DarkMagenta", "DarkRed" } },
                { "LightGradient", new string[] { "White", "Gray", "DarkGray" } },
                { "DarkGradient", new string[] { "Black", "DarkGray", "Gray" } },
                { "All", Enum.GetNames(typeof(ConsoleColor))}
            };

            Konsole This { get; set; }
            public enum SplitMethod { None, Line, Word, Chunk, Char };
            public ConsoleColor Primary { get; set; } = ConsoleColor.White;
            public ConsoleColor Secondary { get; set; } = ConsoleColor.Gray;
            public ConsoleColor Prompt { get; set; } = ConsoleColor.DarkGray;
            public ConsoleColor Previous { get; private set; }
            public bool ForceReset { get; set; } = true; // Reset the color every write when true. Previous color is inherited when false.

            public ConsoleColor Current
            {
                get { return Console.ForegroundColor; }
                set
                {
                    if (Current != value)
                    {
                        if (Previous != Current)
                        {
                            Previous = Current;
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

            public Colors(Konsole instance)
            {
                This = instance;
                Console.ResetColor();
                Current = Primary;
                Previous = Current;
            }

            /// <summary>
            /// Randomize a number from zero to max.
            /// </summary>
            /// <param name="max">The maximum value.</param>
            /// <param name="original">An optional value to avoid.</param>
            /// <returns>Returns an int within the specified range that is not equals to the original value.</returns>
            static int Randomize(int max, int original = -1)
            {
                Random rnd = new Random();
                int i = rnd.Next(max);

                if (original > -1)
                {
                    while (i == original)
                    {
                        i = rnd.Next(max);
                    }
                }

                return i;
            }

            /// <summary>
            /// Check if a string starts with or is a tag.
            /// </summary>
            /// <param name="text">The string to check</param>
            /// <returns>True if a tag is found</returns>
            public static bool StartsWithTag(string text)
            {
                return (Exists(Regex.Match(text, @"^<(\w+)>").Groups[1].Value) || Regex.Match(text, @"^<prompt>").Success) ? true : false;
            }

            /// <summary>
            /// Replace a tag with a new string.
            /// </summary>
            /// <param name="text">The string to modify.</param>
            /// <param name="replace">The string to replace any matches.</param>
            /// <param name="count">The number of tags to replace. It will replace all if 0.</param>
            /// <returns>The modified string.</returns>
            public static string ReplaceTag(string text, string replace, int count = 0)
            {
                int i = 0;

                foreach (Match m in Regex.Matches(text, @"<\w+>"))
                {
                    if (StartsWithTag(m.Value))
                    {
                        text = Regex.Replace(text, m.Value, replace);
                    }

                    i++;

                    if (count > 0 && i == count)
                    {
                        break;
                    }
                }

                return text;
            }

            /// <summary>
            /// An extension to the ReplaceTag method specifically for removing tags, instead.
            /// </summary>
            /// <param name="text">The string to modify.</param>
            /// <param name="count">The number of tags to remove.</param>
            /// <returns>The modified string.</returns>
            public static string RemoveTag(string text, int count = 0)
            {
                return ReplaceTag(text, "", 0);
            }

            /// <summary>
            /// Convert a Palette to a ConsoleColor array.
            /// </summary>
            /// <param name="_palette">The Palette to convert.</param>
            /// <returns>A ConsoleColor array</returns>
            static ConsoleColor[] ConvertPalette(Palette _palette)
            {
                string[] palette = GetPalette(_palette);
                ConsoleColor[] colors = new ConsoleColor[palette.Length];

                for (int i = 0; i < palette.Length; i++)
                {
                    colors[i] = GetColor(palette[i]);
                }

                return colors;
            }

            /// <summary>
            /// Returns a random ConsoleColor
            /// </summary>
            /// <returns>A random ConsoleColor</returns>
            public static ConsoleColor RandomColor(Palette palette = Palette.All)
            {
                ConsoleColor[] colors = ConvertPalette(palette);
                int r = Randomize(colors.Length);

                while (colors[r] == Console.ForegroundColor || colors[r] == Console.BackgroundColor)
                {
                    r = Randomize(colors.Length);
                }

                return colors[r];
            }

            /// <summary>
            /// Checks if ConsoleColor contains a color represented by a string. This is case-insensitive.
            /// </summary>
            /// <param name="color">The color to check.</param>
            /// <returns>True if color is found.</returns>
            public static bool Exists(string color)
            {
                foreach (string consolecolor in Palettes["All"])
                {
                    if (color.ToLower() == consolecolor.ToLower())
                    {
                        return true;
                    }
                }

                return false;
            }

            /// <summary>
            /// Converts a ConsoleColor to its string representation
            /// </summary>
            /// <param name="color">The ConsoleColor to convert</param>
            /// <returns>A ConsoleColor</returns>
            public static string GetColor(ConsoleColor color)
            {
                return color.ToString();
            }

            /// <summary>
            /// Gets the ConsoleColor equivalent of a string.
            /// </summary>
            /// <param name="color">A string representation of a ConsoleColor</param>
            /// <returns>ConsoleColor equivalent of the string or Console.ForegroundColor if equivalent is not found</returns>
            public static ConsoleColor GetColor(string color)
            {
                if (Exists(color))
                {
                    return (ConsoleColor)Enum.Parse(typeof(ConsoleColor), color, true);
                }
                else
                {
                    return Console.ForegroundColor;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="_palette"></param>
            /// <returns></returns>
            public static string[] GetPalette(Palette _palette, int randomCount = 1)
            {
                string palettename = _palette.ToString();

                if (palettename.Contains("Random"))
                {
                    Palette palette;

                    if (palettename.Contains("Light"))
                    {
                        palette = Palette.Light;
                    }
                    else if (palettename.Contains("Dark"))
                    {
                        palette = Palette.Dark;
                    }
                    else
                    {
                        palette = Palette.All;
                    }

                    string[] randomPalette = new string[randomCount];

                    for (int i = 0; i < randomCount; i++)
                    {
                        string color = GetColor(RandomColor(palette));

                        while (i > 0 && randomPalette[i - 1] == color)
                        {
                            color = GetColor(RandomColor(palette));
                        }

                        randomPalette[i] = color;
                    }

                    return randomPalette;
                }
                else if (!palettename.Contains("Wave"))
                {
                    return Palettes[palettename];
                }
                else
                {
                    List<string> p = new List<string>();

                    palettename = palettename.Replace("Wave", "");

                    string[] palette = Palettes[palettename];

                    foreach (string color in palette)
                    {
                        p.Add(color);
                    }

                    Array.Reverse(palette);

                    for (int i = 1; i < palette.Length - 1; i++)
                    {
                        p.Add(palette[i]);
                    }

                    return p.ToArray();
                }
            }

            public static string InsertTag(string text, ConsoleColor color)
            {
                string tag = GetColor(color).Encapsulate("<");

                return tag + text;
            }

            public static string InsertTag(string text, string color)
            {
                return InsertTag(text, GetColor(color));
            }

            public static int CountTags(string text)
            {
                int count = 0;

                foreach (Match m in Regex.Matches(text, @"<\w+>"))
                {
                    if (StartsWithTag(m.Value))
                    {
                        count++;
                    }
                }

                return count;
            }

            public static bool ContainsTag(string text)
            {
                foreach (Match m in Regex.Matches(text, @"<\w+>"))
                {
                    if (StartsWithTag(m.Value))
                    {
                        return true;
                    }
                }

                return false;
            }

            /// <summary>
            /// Split the text before any confirmed color tag &lt;color&gt; or at the end tag &lt;\&gt; so that it can be processed by the Paint method.
            /// </summary>
            /// <param name="text">The string to split</param>
            /// <returns>A string array containing the split text.</returns>
            public static string[] Split(string text)
            {
                text = ReplaceTag(text, @"</>$&");
                text = Regex.Replace(text, @"(</>){2,}", "</>");

                return text.Split("</>", StringSplitOptions.RemoveEmptyEntries);
            }

            /// <summary>
            /// Split a string into chunks of a specific number of characters.
            /// </summary>
            /// <param name="text">The text to split.</param>
            /// <param name="chunkSize">The size of the chunk.</param>
            /// <param name="countWhitespace">If true, whitespaces will be counted as characters.</param>
            /// <returns>A string array.</returns>
            public static string[] SplitChunks(string text, int chunkSize, bool countWhitespace = false)
            {
                List<string> list = new List<string>();
                int i = 0;

                while (i < text.Length)
                {
                    string chunk = "";

                    for (int j = 0; j < chunkSize; j++)
                    {
                        if (i < text.Length)
                        {
                            while (!countWhitespace && Regex.Match(text[i].ToString(), @"\s").Success)
                            {
                                chunk += text[i];
                                i++;
                            }
                        }

                        if (Regex.Match(chunk, @"^\s+$").Success)
                        {
                            list[list.Count - 1] += chunk;
                            chunk = "";
                        }

                        if (i < text.Length)
                        {
                            chunk += text[i];
                        }

                        i++;
                    }

                    list.Add(chunk);
                }

                return list.ToArray();
            }

            public static string[] SplitLines(string text)
            {
                return Regex.Replace(text, @"(\n\s*)", @"$1</>") .Split("</>");
            }

            public static string[] SplitWords(string text)
            {
                return Regex.Replace(text, @"(\s)(\S)", @"$1</>$2").Split("</>");
            }

            public static string[] SplitChars(string text, bool countWhitespace = false)
            {
                return SplitChunks(text, 1, countWhitespace);
            }

            /// <summary>
            /// Toggles the Current color with the specified color
            /// </summary>
            /// <param name="color">The color to toggle to. It will set to Former if null.</param>
            public void Toggle(ConsoleColor? color = null)
            {
                Current = color ?? Previous;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public ConsoleColor GetSwitch()
            {
                if (Current == Primary)
                {
                    return Secondary;
                }
                else
                {
                    return Primary;
                }
            }

            /// <summary>
            /// Switch Current color between Primary and Secondary colors or set to Primary if Current is not any of the two.
            /// </summary>
            public void Switch()
            {
                Current = GetSwitch();
            }

            /// <summary>
            /// Parse the color tag &lt;color&gt; at the beginning of text and change the Current color if it is a match. Ignore if it isn't a known color.
            /// </summary>
            /// <param name="text">The text to colorize.</param>
            /// <returns>The original text sans any confirmed color tags while also cleaning up any literal tags.</returns>
            public string Paint(string text)
            {
                if (ForceReset)
                {
                    Switch();
                }
                else
                {
                    if (This.Prefix.Current == Prefixes.Setting.Prompt || This.Prefix.Current == Prefixes.Setting.Indent)
                    {
                        Toggle();
                    }
                }

                string colortag = Regex.Match(text, @"^<(\w+)>").Groups[1].Value; // Parse the color from the color tag
                
                if (Exists(colortag))
                {
                    Current = GetColor(colortag);
                }
                else if (colortag == "prompt")
                {
                    Console.ForegroundColor = Prompt;
                }

                text = Regex.Replace(text, @"^<\w+>", ""); // Remove the color tag

                return Regex.Replace(text, @"<\\(\w+>)", @"<$1"); // Clean up any forced literal tags from <\text> to <text>
            }
            
            /// <summary>
            /// 
            /// </summary>
            /// <param name="chunks"></param>
            /// <param name="palette"></param>
            /// <param name="randomSort"></param>
            /// <returns></returns>
            public static string[] PaletteInsert(string[] chunks, Palette _palette, bool randomSort = false)
            {
                string[] palette = GetPalette(_palette, chunks.Length);
                
                return PaletteInsert(chunks, palette, randomSort);
            }

            public static string[] PaletteInsert(string[] chunks, string[] palette, bool randomSort = false)
            {
                int i = (randomSort) ? Randomize(palette.Length) : 0;
                string color = "";

                for (int j = 0; j < chunks.Length; j++)
                {
                    if (!Regex.Match(chunks[j], @"^\s*$").Success)
                    {
                        while (i >= palette.Length && !randomSort)
                        {
                            i -= palette.Length;
                        }

                        color = palette[i];

                        i = (randomSort) ? Randomize(palette.Length, i) : i + 1;

                        chunks[j] = InsertTag(chunks[j], color);
                    }
                }

                return chunks;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="text"></param>
            /// <param name="_palette"></param>
            /// <param name="chunkSize"></param>
            /// <param name="randomSort"></param>
            /// <returns></returns>
            public static string[] PaletteChunks(string text, Palette palette, int chunkSize, bool randomSort = false)
            {
                text = RemoveTag(text);

                return PaletteInsert(SplitChunks(text, chunkSize), palette, randomSort);
            }

            public static string[] PaletteChunks(string text, string[] palette, int chunkSize, bool randomSort = false)
            {
                text = RemoveTag(text);

                return PaletteInsert(SplitChunks(text, chunkSize), palette, randomSort);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="text"></param>
            /// <param name="palette"></param>
            /// <param name="randomSort"></param>
            /// <returns></returns>
            public static string[] PaletteLines(string text, Palette palette, bool randomSort = false)
            {
                text = RemoveTag(text);

                return PaletteInsert(SplitLines(text), palette, randomSort);
            }

            public static string[] PaletteLines(string text, string[] palette, bool randomSort = false)
            {
                text = RemoveTag(text);

                return PaletteInsert(SplitLines(text), palette, randomSort);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="text"></param>
            /// <param name="palette"></param>
            /// <param name="randomSort"></param>
            /// <returns></returns>
            public static string[] PaletteWords(string text, Palette palette, bool randomSort = false)
            {
                text = RemoveTag(text);

                return PaletteInsert(SplitWords(text), palette, randomSort);
            }

            public static string[] PaletteWords(string text, string[] palette, bool randomSort = false)
            {
                text = RemoveTag(text);

                return PaletteInsert(SplitWords(text), palette, randomSort);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="text"></param>
            /// <param name="_palette"></param>
            /// <param name="randomSort"></param>
            /// <returns></returns>
            public static string[] PaletteChars(string text, Palette palette, bool randomSort = false)
            {
                return PaletteChunks(text, palette, 1, randomSort);
            }

            public static string[] PaletteChars(string text, string[] palette, bool randomSort = false)
            {
                return PaletteChunks(text, palette, 1, randomSort);
            }
        }
    }
}
