namespace Kontext
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Global;

    public partial class Konsole
    {
        public enum SplitMethod
        {
            None,
            Line,
            Word,
            Chunk,
            Char
        };

        public enum Palette
        {
            Rainbow,
            RainbowWave,
            Light,
            LightWave,
            Dark,
            DarkWave,
            LightMono,
            LightMonoWave,
            DarkMono,
            DarkMonoWave,
            Random,
            RandomLight,
            RandomDark,
            RandomAuto,
            AutoMono,
            AutoMonoWave,
            Auto,
            AutoWave,
            All
        };

        public Parameters.Color Color { get; private set; }

        public partial class Parameters
        {
            public class Color
            {
                static Dictionary<string, string[]> Palettes { get; } = new Dictionary<string, string[]>
            {
                { "Rainbow", new string[] { "Red", "Yellow", "Green", "Cyan", "Magenta" } },
                { "Light", new string[] { "White", "Gray", "Cyan", "Yellow", "Green", "Magenta", "Red" } },
                { "Dark", new string[] { "Black", "DarkGray", "DarkCyan", "Blue", "DarkBlue", "DarkYellow", "DarkGreen", "DarkMagenta", "DarkRed" } },
                { "LightMono", new string[] { "White", "Gray", "DarkGray" } },
                { "DarkMono", new string[] { "Black", "DarkGray", "Gray" } },
                { "All", Enum.GetNames(typeof(ConsoleColor))}
            };

                Konsole Parent { get; set; }
                public ConsoleColor Primary { get; set; } = ConsoleColor.White;
                public ConsoleColor Secondary { get; set; } = ConsoleColor.Gray;
                public ConsoleColor Prompt { get; set; } = ConsoleColor.DarkCyan;
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

                public Color(Konsole parent)
                {
                    Parent = parent;
                    Console.ResetColor();
                    Console.Clear();
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
                public static string Recoat(string text, string replace, int count = 0)
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
                public static string Shake(string text, int count = 0)
                {
                    return Recoat(text, "", count);
                }

                /// <summary>
                /// Convert a Palette to a ConsoleColor array.
                /// </summary>
                /// <param name="palette">The Palette to convert.</param>
                /// <returns>A ConsoleColor array</returns>
                static ConsoleColor[] ConvertPalette(Palette palette)
                {
                    string[] paletteArray = GetPalette(palette);
                    ConsoleColor[] colorArray = new ConsoleColor[paletteArray.Length];

                    for (int i = 0; i < paletteArray.Length; i++)
                    {
                        colorArray[i] = GetColor(paletteArray[i]);
                    }

                    return colorArray;
                }

                /// <summary>
                /// Returns a random ConsoleColor
                /// </summary>
                /// <returns>A random ConsoleColor</returns>
                public static ConsoleColor RandomColor(Palette palette = Palette.All)
                {
                    ConsoleColor[] colorArray = ConvertPalette(palette);
                    int r = Randomize(colorArray.Length);

                    while (colorArray[r] == Console.ForegroundColor || colorArray[r] == Console.BackgroundColor)
                    {
                        r = Randomize(colorArray.Length);
                    }

                    return colorArray[r];
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
                /// <param name="palette"></param>
                /// <returns></returns>
                public static string[] GetPalette(Palette palette, int randomCount = 1)
                {
                    string palettename = palette.ToString();

                    if (palettename.Contains("Random"))
                    {
                        if (palettename.Contains("Light"))
                        {
                            palette = Palette.Light;
                        }
                        else if (palettename.Contains("Dark"))
                        {
                            palette = Palette.Dark;
                        }
                        else if (palettename.Contains("Auto"))
                        {
                            bool darkBackground = BackgroundIsDark();

                            palette = (darkBackground) ? Palette.Light : Palette.Dark;
                        }
                        else
                        {
                            palette = Palette.All;
                        }

                        string[] randomPaletteArray = new string[randomCount];

                        for (int i = 0; i < randomCount; i++)
                        {
                            string color = GetColor(RandomColor(palette));

                            while (i > 0 && randomPaletteArray[i - 1] == color)
                            {
                                color = GetColor(RandomColor(palette));
                            }

                            randomPaletteArray[i] = color;
                        }

                        return randomPaletteArray;
                    }
                    else if (palettename.Contains("Auto"))
                    {
                        bool darkBackground = BackgroundIsDark();

                        palettename = Regex.Replace(palettename, "Auto", ((darkBackground) ? "Light" : "Dark"));

                        return GetPalette(palettename);
                    }
                    else if (!palettename.Contains("Wave"))
                    {
                        return Palettes[palettename];
                    }
                    else
                    {
                        List<string> p = new List<string>();

                        palettename = palettename.Replace("Wave", "");

                        string[] paletteArray = Palettes[palettename];

                        foreach (string color in paletteArray)
                        {
                            p.Add(color);
                        }

                        Array.Reverse(paletteArray);

                        for (int i = 1; i < paletteArray.Length - 1; i++)
                        {
                            p.Add(paletteArray[i]);
                        }

                        return p.ToArray();
                    }
                }

                static bool BackgroundIsDark()
                {
                    foreach (string color in Palettes["Dark"])
                    {
                        if (GetColor(color) == Console.BackgroundColor)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                static string[] GetPalette(string paletteName, int randomCount = 1)
                {
                    Palette palette;

                    try
                    {
                        palette = (Palette)Enum.Parse(typeof(Palette), paletteName, true);
                    }
                    catch
                    {
                        palette = Palette.Auto;
                    }

                    return GetPalette(palette, randomCount);
                }

                public static string Coat(string text, ConsoleColor color)
                {
                    string tag = GetColor(color).Encapsulate("<");

                    return tag + text + "</>";
                }

                public static string Coat(string text, string color)
                {
                    return Coat(text, GetColor(color));
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
                    text = Recoat(text, @"</>$&");
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
                                while (!countWhitespace && i < text.Length && Regex.Match(text[i].ToString(), @"\s").Success)
                                {
                                    chunk += text[i];
                                    i++;
                                }
                            }

                            if (Regex.Match(chunk, @"^\s+$").Success)
                            {
                                if (list.Count > 1)
                                {
                                    list[list.Count - 1] += chunk;
                                }
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
                    return Regex.Replace(text, @"([\r\n]+\s*)", @"$1</>").Split("</>");
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

                public void Reset()
                {
                    Toggle(Primary);
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
                        Reset();
                    }
                    else
                    {
                        if (Parent.Prefix.Current == PrefixType.Prompt || Parent.Prefix.Current == PrefixType.Indent)
                        {
                            Toggle();
                        }
                    }

                    string colortag = Regex.Match(text, @"^<(\w+)>").Groups[1].Value; // Parse the possible color from the tag
                    bool validColor;

                    if (Exists(colortag))
                    {
                        Current = GetColor(colortag);
                        validColor = true;
                    }
                    else
                    {
                        validColor = true;

                        switch (colortag)
                        {
                            case "prompt":
                                Console.ForegroundColor = Prompt;
                                break;
                            case "primary":
                                Console.ForegroundColor = Primary;
                                break;
                            case "secondary":
                                Console.ForegroundColor = Secondary;
                                break;
                            case "random":
                                Console.ForegroundColor = RandomColor(Palette.Auto);
                                break;
                            default:
                                validColor = false;
                                break;
                        }
                    }

                    // Remove tag if tag is a valid color and clean up any forced literal tags from <\text> to <text>
                    return Regex.Replace(((validColor) ? Regex.Replace(text, @"^<\w+>", "") : text), @"<\\(\w+>)", @"<$1");
                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="chunkArray"></param>
                /// <param name="palette"></param>
                /// <param name="randomSort"></param>
                /// <returns></returns>
                public static string[] PaletteInsert(string[] chunkArray, Palette palette, bool randomSort = false)
                {
                    string[] paletteArray = GetPalette(palette, chunkArray.Length);

                    return PaletteInsert(chunkArray, paletteArray, randomSort);
                }

                public static string[] PaletteInsert(string[] chunkArray, string[] paletteArray, bool randomSort = false)
                {
                    int i = (randomSort) ? Randomize(paletteArray.Length) : 0;
                    string color = "";

                    for (int j = 0; j < chunkArray.Length; j++)
                    {
                        if (!Regex.Match(chunkArray[j], @"^\s*$").Success)
                        {
                            while (i >= paletteArray.Length && !randomSort)
                            {
                                i -= paletteArray.Length;
                            }

                            color = paletteArray[i];

                            i = (randomSort) ? Randomize(paletteArray.Length, i) : i + 1;

                            chunkArray[j] = Coat(chunkArray[j], color);
                        }
                    }

                    return chunkArray;
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
                    text = Shake(text);

                    return PaletteInsert(SplitChunks(text, chunkSize), palette, randomSort);
                }

                public static string[] PaletteChunks(string text, string[] paletteArray, int chunkSize, bool randomSort = false)
                {
                    text = Shake(text);

                    return PaletteInsert(SplitChunks(text, chunkSize), paletteArray, randomSort);
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
                    text = Shake(text);

                    return PaletteInsert(SplitLines(text), palette, randomSort);
                }

                public static string[] PaletteLines(string text, string[] paletteArray, bool randomSort = false)
                {
                    text = Shake(text);

                    return PaletteInsert(SplitLines(text), paletteArray, randomSort);
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
                    text = Shake(text);

                    return PaletteInsert(SplitWords(text), palette, randomSort);
                }

                public static string[] PaletteWords(string text, string[] paletteArray, bool randomSort = false)
                {
                    text = Shake(text);

                    return PaletteInsert(SplitWords(text), paletteArray, randomSort);
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

                public static string[] PaletteChars(string text, string[] paletteArray, bool randomSort = false)
                {
                    return PaletteChunks(text, paletteArray, 1, randomSort);
                }
            }
        }
    }
}
