namespace Polymer
{
    using System;
    using System.Collections.Generic;
    using static Methods;

    public partial class Konsole
    {
        public enum SplitMode
        {
            None,
            Line,
            Word,
            Chunk,
            Char
        };

        public partial class Parameters
        {
            public partial class Color
            {
                public class Splitter
                {
                    public struct SplitParameters
                    {
                        public SplitMode Mode
                        {
                            get
                            {
                                if (Parameters is SplitMode)
                                {
                                    return Parameters;
                                }
                                else if (Parameters is int)
                                {
                                    return SplitMode.Chunk;
                                }
                                else
                                {
                                    return SplitMode.None;
                                }
                            }
                        }

                        readonly dynamic Parameters;

                        public SplitParameters(int chunkSize)
                        {
                            if (chunkSize == 1)
                            {
                                Parameters = SplitMode.Char;
                            }
                            else if (chunkSize > 1)
                            {
                                Parameters = chunkSize;
                            }
                            else
                            {
                                Parameters = SplitMode.None;
                            }
                        }

                        public SplitParameters(SplitMode mode)
                        {
                            if (mode != SplitMode.Chunk)
                            {
                                Parameters = mode;
                            }
                            else
                            {
                                Parameters = SplitMode.Char;
                            }
                        }

                        public static implicit operator SplitMode(SplitParameters parameters)
                        {
                            return parameters.Mode;
                        }

                        public static implicit operator SplitParameters(SplitMode? mode)
                        {
                            return new SplitParameters(mode ?? SplitMode.None);
                        }

                        public static implicit operator SplitParameters(int? chunkSize)
                        {
                            return new SplitParameters(chunkSize ?? 0);
                        }

                        public static implicit operator int(SplitParameters parameters)
                        {
                            return (parameters.Parameters is int) ? parameters.Parameters : 0;
                        }
                    }

                    public Konsole Parent { get; set; }

                    // These bool properties defaults to false and must be explicitly set to true if required.
                    public bool CountWhiteSpaces { get; set; } = false; // Only matters with SplitMode.Chunks and SplitMode.Chars
                    public bool RandomSort { get; set; } = false;

                    SplitMode Mode { get => Parameters; }
                    Palette Palette { get => _palette ?? Palette.Auto; }

                    SplitParameters Parameters;
                    Palette? _palette = null;

                    public Splitter(SplitMode mode, Palette palette)
                    {
                        Parameters = mode;
                        _palette = palette;
                    }

                    public Splitter(int chunkSize, Palette palette)
                    {
                        Parameters = chunkSize;
                        _palette = palette;
                    }

                    public Splitter(SplitMode mode)
                    {
                        Parameters = mode;
                        _palette = Palette.Auto;
                    }

                    public Splitter(int chunkSize)
                    {
                        Parameters = chunkSize;
                        _palette = Palette.Auto;
                    }

                    public Splitter()
                    {
                        Parameters = SplitMode.None;
                    }

                    public string Execute(string text, Konsole konsole = null)
                    {
                        Parent = konsole;

                        switch (Mode)
                        {
                            case SplitMode.Line:
                                return PaletteLines(text, Palette);
                            case SplitMode.Word:
                                return PaletteWords(text, Palette);
                            case SplitMode.Chunk:
                                return PaletteChunks(text, Palette, Parameters);
                            case SplitMode.Char:
                                return PaletteChars(text, Palette);
                            default:
                                return text;
                        }
                    }

                    string PaletteInsert(string[] chunkArray, Palette palette)
                    {
                        string[] paletteArray = GetPalette(palette, chunkArray.Length);

                        return PaletteInsert(chunkArray, paletteArray);
                    }

                    string PaletteInsert(string[] chunkArray, string[] paletteArray)
                    {
                        int i = (RandomSort) ? Randomize(paletteArray.Length) : 0;

                        for (int j = 0; j < chunkArray.Length; j++)
                        {
                            if (!RegexZeroOrMoreWhitespaces.Match(chunkArray[j]).Success)
                            {
                                while (i >= paletteArray.Length && !RandomSort)
                                {
                                    i -= paletteArray.Length;
                                }

                                ConsoleColor color = GetColor(paletteArray[i]);

                                i = (RandomSort) ? Randomize(paletteArray.Length, i) : i + 1;

                                chunkArray[j] = InsertTag(chunkArray[j], color);
                            }
                        }

                        return chunkArray.Join();
                    }

                    string[] SplitChunks(string text, int chunkSize)
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
                                    while (!CountWhiteSpaces && i < text.Length && RegexWhitespace.Match(text[i].ToString()).Success)
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

                    string[] SplitLines(string text)
                    {
                        return RegexMultipleLinebreaks.Replace(text, @"$1</>").Split("</>");
                    }

                    string[] SplitWords(string text)
                    {
                        return RegexSplitWords.Replace(text, @"$1</>$2").Split("</>");
                    }

                    string[] SplitChars(string text)
                    {
                        return SplitChunks(text, 1);
                    }

                    string PaletteChunks(string text, Palette palette, int chunkSize)
                    {
                        text = CleanTags(text);

                        return PaletteInsert(SplitChunks(text, chunkSize), palette);
                    }

                    /// <summary>
                    /// 
                    /// </summary>
                    /// <param name="text"></param>
                    /// <param name="palette"></param>
                    /// <param name="RandomSort"></param>
                    /// <returns></returns>
                    string PaletteLines(string text, Palette palette)
                    {
                        text = CleanTags(text);

                        return PaletteInsert(SplitLines(text), palette);
                    }

                    /// <summary>
                    /// 
                    /// </summary>
                    /// <param name="text"></param>
                    /// <param name="palette"></param>
                    /// <param name="RandomSort"></param>
                    /// <returns></returns>
                    string PaletteWords(string text, Palette palette)
                    {
                        text = CleanTags(text);

                        return PaletteInsert(SplitWords(text), palette);
                    }

                    /// <summary>
                    /// 
                    /// </summary>
                    /// <param name="text"></param>
                    /// <param name="palette"></param>
                    /// <param name="RandomSort"></param>
                    /// <returns></returns>
                    string PaletteChars(string text, Palette palette)
                    {
                        return PaletteChunks(text, palette, 1);
                    }
                }
            }
        }
    }
}
