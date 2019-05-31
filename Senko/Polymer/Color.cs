namespace Polymer
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Generic;

    using static Generic.Extensions;
    using static Generic.Methods;
    using static Generic.Fields;
    using static Kontext;

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

        /// <summary>
        /// The main Parameters.Color object available to the Konsole object and its subclasses
        /// </summary>
        public Parameters.Color Color { get; private set; }

        public partial class Parameters
        {
            public class Color
            {
                #region PROPERTIES

                /// <summary>
                /// The parent console class for access to non-static methods and fields
                /// </summary>
                Konsole Parent { get; set; }

                /// <summary>
                /// The switch to reset the color every write operation. Previously used color is inherited when false.
                /// </summary>
                public bool ForceColorReset { get; set; } = true;

                /// <summary>
                /// The primary or main color of the instance.
                /// </summary>
                public ConsoleColor PrimaryColor { get; set; } = ConsoleColor.White;

                /// <summary>
                /// The secondary or alternate color of the instance.
                /// </summary>
                public ConsoleColor SecondaryColor { get; set; } = ConsoleColor.Gray;

                /// <summary>
                /// The default color of the instance for use with the prompt string in Parameters.Prefix.
                /// </summary>
                public ConsoleColor PromptColor { get; set; } = ConsoleColor.DarkCyan;

                /// <summary>
                /// The previous color used.
                /// </summary>
                public static ConsoleColor PreviousColor { get; private set; }

                /// <summary>
                /// Get or set the current Console.ForegroundColor. Before setting, the current color is saved to PreviousColor.
                /// </summary>
                public static ConsoleColor ForegroundColor
                {
                    get => Console.ForegroundColor;
                    set
                    {
                        if (ForegroundColor != value)
                        {
                            if (PreviousColor != ForegroundColor)
                            {
                                PreviousColor = ForegroundColor;
                            }

                            Console.ForegroundColor = value;
                        }
                    }
                }

                /// <summary>
                /// Gets or sets Console.BackgroundColor. When setting, Console.Clear() is also called.
                /// </summary>
                public static ConsoleColor BackgroundColor
                {
                    get => Console.BackgroundColor;
                    set
                    {
                        if (BackgroundColor != value)
                        {
                            Console.BackgroundColor = value;
                            Console.Clear();
                        }
                    }
                } 
                
                /// <summary>
                /// A dictionary of ConsoleColor values.
                /// </summary>
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
                static Regex RegexTags { get; } = new Regex(@"<\w+>", RegexOptions.Compiled); // Generic tags: <tag>
                static Regex RegexTagAtStart { get; } = new Regex(@"^<(\w+)>", RegexOptions.Compiled); // Generic tag at start of string
                static Regex RegexDisabledTags { get; } = new Regex(@"<\\(\w+>)", RegexOptions.Compiled); // Disabled tags: <\tag> 
                static Regex RegexExtendedDisabledTags { get; } = new Regex(@"<\\(\\\w+>)", RegexOptions.Compiled); // Extended disabled tags: <\\tag>
                static Regex RegexCountDisabledTags { get; } = new Regex(@"<\\{1,2}\w+>", RegexOptions.Compiled); // Disabled and extended disabled tags
                static Regex RegexMultipleSplitterTags { get; } = new Regex(@"(</>){2,}", RegexOptions.Compiled); // More than 1 splitter tags (</>) in succession
                static Regex RegexZeroOrMoreWhitespaces { get; } = new Regex(@"^\s*$", RegexOptions.Compiled); // Zero or more whitespaces from start to end of string
                static Regex RegexOneOrMoreWhitespaces { get; } = new Regex(@" ^\s+$", RegexOptions.Compiled); // One or more whitespaces from start to end of string
                static Regex RegexWhitespace { get; } = new Regex(@"\s", RegexOptions.Compiled); // Any whitespace character
                static Regex RegexMultipleLinebreaks { get; } = new Regex(@"([\r\n]+\s*)", RegexOptions.Compiled); // Multiple \r, \n or \r\n linebreaks
                static Regex RegexSplitWords { get; } = new Regex(@"(\s)(\S)", RegexOptions.Compiled); // Whitespace and non-whitespace in succession
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
                    ForegroundColor = PrimaryColor;
                    PreviousColor = ForegroundColor;
                }

                /// <summary>
                /// Toggles the Current color with the specified color
                /// </summary>
                /// <param name="color">The color to toggle to. It will set to Previous if null.</param>
                public void Switch(ConsoleColor? color = null)
                {
                    ForegroundColor = color ?? PreviousColor;
                }

                /// <summary>
                /// Switch Current color between Primary and Secondary colors or set to Primary if Current is not any of the two.
                /// </summary>
                public void Toggle()
                {
                    if (ForegroundColor == PrimaryColor)
                    {
                        ForegroundColor = SecondaryColor;
                    }
                    else
                    {
                        ForegroundColor = PrimaryColor;
                    }
                }

                /// <summary>
                /// 
                /// </summary>
                public void Reset()
                {
                    Switch(PrimaryColor);
                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="color"></param>
                /// <param name="konsole"></param>
                /// <returns></returns>
                public static bool Exists(string color, Konsole konsole = null)
                {
                    konsole = konsole ?? MainKonsole;

                    color = TranslateColor(color, konsole);

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
                    return color.ToString().ToLower();
                }

                /// <summary>
                /// Gets the ConsoleColor equivalent of a string.
                /// </summary>
                /// <param name="color">A string representation of a ConsoleColor</param>
                /// <returns>ConsoleColor equivalent of the string or Console.ForegroundColor if equivalent is not found</returns>
                public static ConsoleColor GetColor(string color, Konsole konsole = null)
                {
                    konsole = konsole ?? MainKonsole;

                    color = TranslateColor(color, konsole);

                    if (Exists(color))
                    {
                        return (ConsoleColor)Enum.Parse(typeof(ConsoleColor), color, true);
                    }
                    else
                    {
                        return ForegroundColor;
                    }
                }

                public static string TranslateColor(string color, Konsole konsole = null)
                {
                    konsole = konsole ?? MainKonsole;

                    switch (color)
                    {
                        case "prompt":
                            color = GetColor(konsole.Color.PromptColor);
                            break;
                        case "indent":
                            goto case "primary";
                        case "primary":
                            color = GetColor(konsole.Color.PrimaryColor);
                            break;
                        case "secondary":
                            color = GetColor(konsole.Color.SecondaryColor);
                            break;
                        case "random":
                            color = GetColor(RandomColor(Palette.Auto));
                            break;
                    }

                    return color;
                }

                /// <summary>
                /// Check if a string starts with or is a tag.
                /// </summary>
                /// <param name="text">The string to check</param>
                /// <returns>True if a tag is found</returns>
                public static bool StartsWithTag(string text, Konsole konsole = null)
                {
                    konsole = konsole ?? MainKonsole;

                    return Exists(RegexTagAtStart.Match(text).Groups[1].Value, konsole);
                }

                /// <summary>
                /// Check if text contains a known tag
                /// </summary>
                /// <param name="text"></param>
                /// <returns></returns>
                public static bool ContainsTag(string text, Konsole konsole = null)
                {
                    konsole = konsole ?? MainKonsole;

                    foreach (Match m in RegexTags.Matches(text))
                    {
                        if (StartsWithTag(m.Value, konsole))
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
                        if (GetColor(color) == BackgroundColor)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                public static string CreateTag(object obj)
                {
                    return (obj != null) ? obj.ToString().ToLower().Standardize().Encapsulate(Encapsulator.Chevrons) : "";
                }

                public static string InsertTag(string text, ConsoleColor color, bool addBreakTag = true)
                {
                    return CreateTag(color) + text + ((addBreakTag) ? "</>" : "");
                }

                public static string InsertTag(string text, string pattern, ConsoleColor color, bool addBreakTag = true)
                {
                    return Regex.Replace(text, pattern, InsertTag("$&", color, addBreakTag));
                }

                public static string InsertTag(string text, string pattern, ConsoleColor color, string append)
                {
                    return Regex.Replace(text, pattern, InsertTag("$&" + append, color, false));
                }

                public static string InsertTag(object obj, ConsoleColor color, bool addBreakTag = true)
                {
                    return InsertTag(obj.ToString(), color, addBreakTag);
                }

                public static string InsertTag(object obj, ConsoleColor? color, bool addBreakTag = true)
                {
                    return ((color != null) ? Color.CreateTag(color) : "") + obj.ToString() + ((addBreakTag) ? "</>" : "");
                }

                /// <summary>
                /// Replace a tag with a new string.
                /// </summary>
                /// <param name="text">The string to modify.</param>
                /// <param name="replace">The string to replace any matches.</param>
                /// <param name="count">The number of tags to replace. It will replace all if 0.</param>
                /// <returns>The modified string.</returns>
                public static string ReplaceTags(string text, string replace, int count, Konsole konsole = null)
                {
                    int i = 0;

                    konsole = konsole ?? MainKonsole;

                    foreach (Match m in RegexTags.Matches(text))
                    {
                        if (StartsWithTag(m.Value, konsole))
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

                public static string ReplaceTags(string text, string replace, Konsole konsole = null)
                {
                    return ReplaceTags(text, replace, 0, konsole);
                }

                public static string CleanTags(string text, int count, Konsole konsole = null)
                {
                    int i = 0;

                    konsole = konsole ?? MainKonsole;

                    foreach (Match m in RegexTags.Matches(text))
                    {
                        if (StartsWithTag(m.Value, konsole))
                        {
                            text = text.Replace(m.Value, "");
                        }

                        i++;

                        if (count > 0 && i == count)
                        {
                            break;
                        }
                    }

                    return RegexDisabledTags.Replace(text, "<$1");
                }

                public static string CleanTags(string text, Konsole konsole = null)
                {
                    return CleanTags(text, 0, konsole);
                }

                public static string DisableTags(string text, Konsole konsole = null)
                {
                    text = RegexDisabledTags.Replace(text, @"<\\$1");

                    konsole = konsole ?? MainKonsole;

                    foreach (Match m in RegexTags.Matches(text))
                    {
                        if (StartsWithTag(m.Value, konsole))
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

                public static int CountTags(string text, Konsole konsole = null)
                {
                    int count = 0;

                    konsole = konsole ?? MainKonsole;

                    foreach (Match m in RegexTags.Matches(text))
                    {
                        if (StartsWithTag(m.Value, konsole))
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
                public static string Paint(string text, Konsole konsole = null)
                {
                    string colortag = RegexTagAtStart.Match(text).Groups[1].Value; // Parse the possible color from the tag
                    bool validColor = Exists(colortag, konsole);

                    if (validColor)
                    {
                        ForegroundColor = GetColor(colortag, konsole);
                    }
                    else
                    {
                        ForegroundColor = konsole.Color.PrimaryColor;
                    }

                    // Remove tag if tag is a valid color and clean up any forced literal tags from <\text> to <text>
                    return CleanTags((validColor) ? RegexTagAtStart.Replace(text, "") : text, konsole);
                }

                public static void AddCustomPalette(string name, params ConsoleColor[] colors)
                {
                    int i = 1;

                    name = "Custom" + ((name != "") ? "." + name : "");

                    string tempName = name;

                    while (Palettes.ContainsKey(tempName))
                    {
                        tempName = name + "." + i;
                        i++;
                    }

                    name = tempName;

                    var palette = new List<string>();

                    foreach(ConsoleColor color in colors)
                    {
                        palette.Add(GetColor(color));
                    }

                    Palettes.Add(name, palette.ToArray());

                    string numberOfColors = palette.Count + ((palette.Count > 1) ? " colors" : " color");
                    string colorList = "";

                    i = 1;

                    foreach (string color in palette)
                    {
                        colorList = colorList + Environment.NewLine + i + ". " + GetColor(color).ToString();
                        i++;
                    }

                    MainInfoKonsole.WriteLine();
                    MainInfoKonsole.WriteLine("Custom palette {0} with {1} has been successfully added:{3}", name.Encapsulate("\""), numberOfColors, PrefixType.Prompt, colorList);
                }

                public static void AddCustomPalette(params ConsoleColor[] colors)
                {
                    AddCustomPalette("", colors);
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
                /// Returns a random ConsoleColor
                /// </summary>
                /// <returns>A random ConsoleColor</returns>
                public static ConsoleColor RandomColor(Palette palette = Palette.All)
                {
                    ConsoleColor[] colorArray = ConvertPalette(palette);
                    int r = Randomize(colorArray.Length);

                    while (colorArray[r] == ForegroundColor || colorArray[r] == BackgroundColor)
                    {
                        r = Randomize(colorArray.Length);
                    }

                    return colorArray[r];
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
                /// <param name="palette"></param>
                /// <param name="chunkSize"></param>
                /// <param name="randomSort"></param>
                /// <returns></returns>
                public static string[] PaletteChunks(string text, Palette palette, int chunkSize, bool randomSort, Konsole konsole = null)
                {
                    text = CleanTags(text, konsole);

                    return PaletteInsert(SplitChunks(text, chunkSize), palette, randomSort);
                }

                public static string[] PaletteChunks(string text, Palette palette, int chunkSize, Konsole konsole = null)
                {
                    return PaletteChunks(text, palette, chunkSize, false, konsole);
                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="text"></param>
                /// <param name="palette"></param>
                /// <param name="randomSort"></param>
                /// <returns></returns>
                public static string[] PaletteLines(string text, Palette palette, bool randomSort, Konsole konsole = null)
                {
                    text = CleanTags(text, konsole);

                    return PaletteInsert(SplitLines(text), palette, randomSort);
                }

                public static string[] PaletteLines(string text, Palette palette, Konsole konsole = null)
                {
                    return PaletteLines(text, palette, false, konsole);
                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="text"></param>
                /// <param name="palette"></param>
                /// <param name="randomSort"></param>
                /// <returns></returns>
                public static string[] PaletteWords(string text, Palette palette, bool randomSort, Konsole konsole = null)
                {
                    text = CleanTags(text, konsole);

                    return PaletteInsert(SplitWords(text), palette, randomSort);
                }

                public static string[] PaletteWords(string text, Palette palette, Konsole konsole = null)
                {
                    return PaletteWords(text, palette, false, konsole);
                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="text"></param>
                /// <param name="palette"></param>
                /// <param name="randomSort"></param>
                /// <returns></returns>
                public static string[] PaletteChars(string text, Palette palette, bool randomSort, Konsole konsole = null)
                {
                    return PaletteChunks(text, palette, 1, randomSort, konsole);
                }

                public static string[] PaletteChars(string text, Palette palette, Konsole konsole = null)
                {
                    return PaletteChunks(text, palette, 1, false, konsole);
                }

                /// <summary>
                /// Split the text before any confirmed color tag &lt;color&gt; or at the end tag &lt;\&gt; so that it can be processed by the Paint method.
                /// </summary>
                /// <param name="text">The string to split</param>
                /// <returns>A string array containing the split text.</returns>
                public static string[] Split(string text, Konsole konsole = null)
                {
                    text = ReplaceTags(text, @"</>$&", konsole);
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

                public class SplitParameters
                {
                    Konsole konsole { get; set; } = null;
                    public SplitMethod Method { get; private set; }
                    public Palette Palette { get; private set; }
                    public int ChunkSize { get; private set; }

                    public SplitParameters(SplitMethod method, Palette? palette, int chunkSize, Konsole konsole)
                    {
                        SetParameters(method, palette, chunkSize, konsole);
                    }

                    public SplitParameters(SplitMethod method, Palette? palette, int chunkSize = 0)
                    {
                        SetParameters(method, palette, chunkSize);
                    }

                    public SplitParameters(SplitMethod method, Konsole konsole = null)
                    {
                        SetParameters(method, null, konsole);
                    }

                    public SplitParameters(Palette palette, Konsole konsole = null)
                    {
                        SetParameters(SplitMethod.Char, palette, konsole);
                    }

                    public SplitParameters(int chunkSize, Konsole konsole = null)
                    {
                        SetParameters(SplitMethod.Chunk, null, chunkSize, konsole);
                    }

                    public SplitParameters()
                    {
                        SetParameters(SplitMethod.None, null);
                    }

                    void SetParameters(SplitMethod method, Palette? palette, int chunkSize, Konsole konsole = null)
                    {
                        if (chunkSize == 0 && method == SplitMethod.Chunk)
                        {
                            method = SplitMethod.Word;
                        }
                        else if (chunkSize > 0 && method != SplitMethod.Chunk)
                        {
                            method = SplitMethod.Chunk;
                        }

                        Method = method;

                        if (method != SplitMethod.None)
                        {
                            Palette = palette ?? Palette.Auto;

                            if (chunkSize > 0)
                            {
                                ChunkSize = chunkSize;
                            }
                        }
                    }

                    void SetParameters(SplitMethod method, Palette? palette, Konsole konsole = null)
                    {
                        SetParameters(method, palette, 0, konsole);
                    }

                    public string Execute(string text)
                    {
                        switch (Method)
                        {
                            case SplitMethod.Line:
                                return Color.PaletteLines(text, Palette, konsole).Join();
                            case SplitMethod.Word:
                                return Color.PaletteWords(text, Palette, konsole).Join();
                            case SplitMethod.Chunk:
                                return Color.PaletteChunks(text, Palette, ChunkSize, konsole).Join();
                            case SplitMethod.Char:
                                return Color.PaletteChars(text, Palette, konsole).Join();                                
                            default:
                                return text;
                        }
                    }
                }
            }
        }
    }
}
