namespace Kontext
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Generic;

    using static Generic.Extensions;
    using static Generic.Methods;
    using static Generic.Fields;

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
                #region PROPERTIES
                Konsole Parent { get; set; } // The parent console class for non-static methods
                public bool ForceReset { get; set; } = true; // Reset the color every write when true. Previous color is inherited when false.
                public ConsoleColor Primary { get; set; } = ConsoleColor.White; // Get or set the primary Console.ForegroundColor
                public ConsoleColor Secondary { get; set; } = ConsoleColor.Gray; // Get or set the secondary Console.ForegroundColor
                public ConsoleColor Prompt { get; set; } = ConsoleColor.DarkCyan; //  Get or set the default prompt Console.ForegroundColor

                /// <summary>
                /// Get the previous color.
                /// </summary>
                public static ConsoleColor Previous { get; private set; } // Get the previous color

                /// <summary>
                /// Get or set the current Console.ForegroundColor
                /// </summary>
                public static ConsoleColor Current
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

                public static ConsoleColor Background // Get or set the Console.BackgroundColor
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
                
                static Dictionary<string, string[]> Palettes { get; } = new Dictionary<string, string[]>
                {
                    { "Rainbow", new string[] { "Red", "Yellow", "Green", "Cyan", "Magenta" } },
                    { "Light", new string[] { "White", "Gray", "Cyan", "Yellow", "Green", "Magenta", "Red" } },
                    { "Dark", new string[] { "Black", "DarkGray", "DarkCyan", "Blue", "DarkBlue", "DarkYellow", "DarkGreen", "DarkMagenta", "DarkRed" } },
                    { "LightMono", new string[] { "White", "Gray", "DarkGray" } },
                    { "DarkMono", new string[] { "Black", "DarkGray", "Gray" } },
                    { "All", Enum.GetNames(typeof(ConsoleColor))}
                };
                #endregion

                #region REGEX OBJECTS: All Regex objects used privately in this class
                static Regex RegexTags { get; } = new Regex(@"<\w+>"); // Generic tags: <tag>
                static Regex RegexTagAtStart { get; } = new Regex(@"^<(\w+)>"); // Generic tag at start of string
                static Regex RegexPromptTag { get; } = new Regex(@"^<prompt>"); // Prompt tag at start of string: <prompt>
                static Regex RegexDisabledTags { get; } = new Regex(@"<\\(\w+>)"); // Disabled tags: <\tag> 
                static Regex RegexExtendedDisabledTags { get; } = new Regex(@"<\\(\\\w+>)"); // Extended disabled tags: <\\tag>
                static Regex RegexCountDisabledTags { get; } = new Regex(@"<\\{1,2}\w+>"); // Disabled and extended disabled tags
                static Regex RegexMultipleSplitterTags { get; } = new Regex(@"(</>){2,}"); // More than 1 splitter tags (</>) in succession
                static Regex RegexZeroOrMoreWhitespaces { get; } = new Regex(@"^\s*$"); // Zero or more whitespaces from start to end of string
                static Regex RegexOneOrMoreWhitespaces { get; } = new Regex(@" ^\s+$"); // One or more whitespaces from start to end of string
                static Regex RegexWhitespace { get; } = new Regex(@"\s"); // Any whitespace character
                static Regex RegexMultipleLinebreaks { get; } = new Regex(@"([\r\n]+\s*)"); // Multiple \r, \n or \r\n linebreaks
                static Regex RegexSplitWords { get; } = new Regex(@"(\s)(\S)"); // Whitespace and non-whitespace in succession
                #endregion

                /// <summary>
                /// The constructor method that initializes the Parent and color fields.
                /// </summary>
                /// <param name="parent">The parent Konsole class</param>
                public Color(Konsole parent)
                {
                    Parent = parent;
                    Console.ResetColor();
                    Console.Clear();
                    Current = Primary;
                    Previous = Current;
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
                /// Check if a string starts with or is a tag.
                /// </summary>
                /// <param name="text">The string to check</param>
                /// <returns>True if a tag is found</returns>
                public static bool StartsWithTag(string text)
                {
                    return (Exists(RegexTagAtStart.Match(text).Groups[1].Value) || RegexPromptTag.Match(text).Success) ? true : false;
                }

                /// <summary>
                /// Check if text contains a known tag
                /// </summary>
                /// <param name="text"></param>
                /// <returns></returns>
                public static bool ContainsTag(string text)
                {
                    foreach (Match m in RegexTags.Matches(text))
                    {
                        if (StartsWithTag(m.Value))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                /// <summary>
                /// Check if Console.BackgroundColor is dark
                /// </summary>
                /// <returns></returns>
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

                public static string CreateTag(object obj)
                {
                    string tag = (obj is ConsoleColor color) ? GetColor(color) : obj.ToString();

                    return tag.Standardize().Encapsulate(Encapsulator.Chevrons);
                }

                public static string InsertTag(string text, ConsoleColor color, bool addBreakTag = true)
                {
                    string tag = CreateTag(color);
                    string breakTag = "</>";

                    if (!addBreakTag)
                    {
                        breakTag = "";
                    }

                    return tag + text + breakTag;
                }

                public static string InsertTag(string text, string pattern, ConsoleColor color, bool addBreakTag = true)
                {
                    return Regex.Replace(text, pattern, InsertTag("$&", color, addBreakTag));
                }

                public static string InsertTag(string text, string pattern, ConsoleColor color, string append)
                {
                    return Regex.Replace(text, pattern, InsertTag("$&" + append, color, false));
                }

                /// <summary>
                /// Replace a tag with a new string.
                /// </summary>
                /// <param name="text">The string to modify.</param>
                /// <param name="replace">The string to replace any matches.</param>
                /// <param name="count">The number of tags to replace. It will replace all if 0.</param>
                /// <returns>The modified string.</returns>
                public static string ReplaceTags(string text, string replace, int count = 0)
                {
                    int i = 0;

                    foreach (Match m in RegexTags.Matches(text))
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

                public static string CleanTags(string text, int count = 0)
                {
                    foreach (Match m in RegexTags.Matches(text))
                    {
                        if (StartsWithTag(m.Value))
                        {
                            text = text.Replace(m.Value, "");
                        }
                    }

                    return RegexDisabledTags.Replace(text, "<$1");
                }

                public static string DisableTags(string text)
                {
                    text = RegexDisabledTags.Replace(text, @"<\\$1");

                    foreach (Match m in RegexTags.Matches(text))
                    {
                        if (StartsWithTag(m.Value))
                        {
                            text = text.Replace(m.Value, m.Value.Replace("<", @"<\"));
                        }
                    }

                    return text;
                }

                public static string EnableTags(string text)
                {
                    text = RegexDisabledTags.Replace(text, @"<$1");
                    return RegexExtendedDisabledTags.Replace(text, @"<$1");
                }

                public static int CountTags(string text)
                {
                    int count = 0;

                    foreach (Match m in RegexTags.Matches(text))
                    {
                        if (StartsWithTag(m.Value))
                        {
                            count++;
                        }
                    }

                    return count;
                }
                public static int CountDisabledTags(string text)
                {
                    int count = 0;

                    foreach (Match m in RegexCountDisabledTags.Matches(text))
                    {
                        count++;
                    }

                    return count;
                }

                /// <summary>
                /// Parse the color tag &lt;color&gt; at the beginning of text and change the Current color if it is a match. Ignore if it isn't a known color.
                /// </summary>
                /// <param name="text">The text to colorize.</param>
                /// <returns>The original text sans any confirmed color tags while also cleaning up any literal tags.</returns>
                public string Paint(string text)
                {
                    string colortag = RegexTagAtStart.Match(text).Groups[1].Value; // Parse the possible color from the tag
                    bool validColor = Exists(colortag);

                    if (!validColor)
                    {
                        validColor = true;

                        switch (colortag)
                        {
                            case "prompt":
                                text = ReplaceTags(text, CreateTag(Prompt));
                                break;
                            case "primary":
                                text = ReplaceTags(text, CreateTag(Primary));
                                break;
                            case "secondary":
                                text = ReplaceTags(text, CreateTag(Secondary));
                                break;
                            case "random":
                                text = ReplaceTags(text, CreateTag(RandomColor(Palette.Auto)));
                                break;
                            default:
                                validColor = false;
                                break;
                        }
                    }

                    return Paint(text, validColor);
                }

                public static string Paint(string text, bool validColor = false)
                {
                    string colortag = RegexTagAtStart.Match(text).Groups[1].Value; // Parse the possible color from the tag

                    validColor = (!validColor) ? Exists(colortag) : validColor;

                    if (validColor)
                    {
                        Current = GetColor(colortag);
                    }

                    // Remove tag if tag is a valid color and clean up any forced literal tags from <\text> to <text>
                    return CleanTags((validColor) ? RegexTagAtStart.Replace(text, "") : text);
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

                        var randomPaletteArray = new string[randomCount];

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

                        palettename = palettename.Replace("Auto", ((darkBackground) ? "Light" : "Dark"));

                        return GetPalette(palettename);
                    }
                    else if (!palettename.Contains("Wave"))
                    {
                        return Palettes[palettename];
                    }
                    else
                    {
                        var p = new List<string>();

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

                /// <summary>
                /// Convert a Palette to a ConsoleColor array.
                /// </summary>
                /// <param name="palette">The Palette to convert.</param>
                /// <returns>A ConsoleColor array</returns>
                static ConsoleColor[] ConvertPalette(Palette palette)
                {
                    string[] paletteArray = GetPalette(palette);
                    var colorArray = new ConsoleColor[paletteArray.Length];

                    for (int i = 0; i < paletteArray.Length; i++)
                    {
                        colorArray[i] = GetColor(paletteArray[i]);
                    }

                    return colorArray;
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

                    for (int j = 0; j < chunkArray.Length; j++)
                    {
                        if (!RegexZeroOrMoreWhitespaces.Match(chunkArray[j]).Success)
                        {
                            while (i >= paletteArray.Length && !randomSort)
                            {
                                i -= paletteArray.Length;
                            }

                            ConsoleColor color = GetColor(paletteArray[i]);

                            i = (randomSort) ? Randomize(paletteArray.Length, i) : i + 1;

                            chunkArray[j] = InsertTag(chunkArray[j], color);
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
                    text = CleanTags(text);

                    return PaletteInsert(SplitChunks(text, chunkSize), palette, randomSort);
                }

                public static string[] PaletteChunks(string text, string[] paletteArray, int chunkSize, bool randomSort = false)
                {
                    text = CleanTags(text);

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
                    text = CleanTags(text);

                    return PaletteInsert(SplitLines(text), palette, randomSort);
                }

                public static string[] PaletteLines(string text, string[] paletteArray, bool randomSort = false)
                {
                    text = CleanTags(text);

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
                    text = CleanTags(text);

                    return PaletteInsert(SplitWords(text), palette, randomSort);
                }

                public static string[] PaletteWords(string text, string[] paletteArray, bool randomSort = false)
                {
                    text = CleanTags(text);

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
                /// Split the text before any confirmed color tag &lt;color&gt; or at the end tag &lt;\&gt; so that it can be processed by the Paint method.
                /// </summary>
                /// <param name="text">The string to split</param>
                /// <returns>A string array containing the split text.</returns>
                public static string[] Split(string text)
                {
                    text = ReplaceTags(text, @"</>$&");
                    text = RegexMultipleSplitterTags.Replace(text, "</>");

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
                    var list = new List<string>();
                    int i = 0;

                    while (i < text.Length)
                    {
                        string chunk = "";

                        for (int j = 0; j < chunkSize; j++)
                        {
                            if (i < text.Length)
                            {
                                while (!countWhitespace && i < text.Length && RegexWhitespace.Match(text[i].ToString()).Success)
                                {
                                    chunk += text[i];
                                    i++;
                                }
                            }

                            if (RegexOneOrMoreWhitespaces.Match(chunk).Success)
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
                    return RegexMultipleLinebreaks.Replace(text, @"$1</>").Split("</>");
                }

                public static string[] SplitWords(string text)
                {
                    return RegexSplitWords.Replace(text, @"$1</>$2").Split("</>");
                }

                public static string[] SplitChars(string text, bool countWhitespace = false)
                {
                    return SplitChunks(text, 1, countWhitespace);
                }
            }
        }
    }
}
