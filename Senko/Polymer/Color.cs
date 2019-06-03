namespace Polymer
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using static Static;
    using static Extensions;
    using static Konsole.Parameters;
    using static Methods;
    using static Polymer;

    public static partial class Polymer
    {
        public static string AppendLine(this object obj, object append, ConsoleColor color)
        {
            return obj.AppendLine(append.InsertTag(color));
        }

        public static string InsertTag(this object obj, ConsoleColor color)
        {
            return Color.InsertTag(obj, color, false);
        }
    }

    public partial class Konsole
    {
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
        public Color Color { get; private set; }

        public partial class Parameters
        {
            public partial class Color
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
                public static Regex RegexTags { get; } = new Regex(@"<\w+>", RegexOptions.Compiled); // Generic tags: <tag>
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
                    return (obj != null) ? obj.ToString().ToLower().Standardize().Encapsulate(EncapsulatorType.Chevrons) : "";
                }

                public static string InsertTag(object obj, ConsoleColor color, bool addBreakTag = true)
                {
                    return obj.ToString().Encapsulate(new Encapsulator(CreateTag(color), (addBreakTag) ? "</>" : ""));
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

                public static string[] Expand(string text, Konsole konsole = null)
                {
                    text = ReplaceTags(text, @"</>$&", konsole);
                    text = RegexMultipleSplitterTags.Replace(text, "</>");

                    return text.Split("</>", StringSplitOptions.RemoveEmptyEntries);
                }
            }
        }
    }
}
