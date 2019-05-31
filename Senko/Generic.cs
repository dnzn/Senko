namespace Generic
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using Polymer;

    using static Fields;

    public static class Fields
    {
        public static Konsole Kon = new Konsole(nameof(Kon));
        public static Konsole KonInfo = new Konsole(nameof(KonInfo));
        public static Konsole KonError = new Konsole(nameof(KonError));

        public enum UnitOfTime
        {
            Milliseconds,
            Seconds,
            Minutes
        };

        public static Dictionary<string, int> Numerals { get; } = new Dictionary<string, int>
        {
            { "zero", 0 },
            { "one", 1 },
            { "two", 2 },
            { "three", 3 },
            { "four", 4 },
            { "five", 5 },
            { "six", 6 },
            { "seven", 7 },
            { "eight", 8 },
            { "nine", 9 }
        };

        public static Dictionary<string, object> Homophones { get; } = new Dictionary<string, object>
        {
            { "won", 1 },
            { "to", 2 },
            { "too", 2 },
            { "tree", 3 },
            { "for", 4 },
            { "ate", 8 },
        };

        public enum Encapsulator
        {
            Parenthesis,
            Brackets,
            Braces,
            Chevrons
        }

        public static Dictionary<char, char> Encapsulators { get; } = new Dictionary<char, char>(); // This is automatically filled

        public static Dictionary<Encapsulator, Dictionary<string, char>> EncapsulatorDictionary { get; } = new Dictionary<Encapsulator, Dictionary<string, char>>
        {
            { Encapsulator.Parenthesis, FillEncapsulatorDictionaries('(', ')') },
            { Encapsulator.Brackets, FillEncapsulatorDictionaries('[', ']') },
            { Encapsulator.Braces, FillEncapsulatorDictionaries('{', '}') },
            { Encapsulator.Chevrons, FillEncapsulatorDictionaries('<', '>') },
        };

        static Dictionary<string, char> FillEncapsulatorDictionaries(char opening, char closing)
        {
            Encapsulators.Add(opening, closing);

            return new Dictionary<string, char>
            {
                { "opening", opening },
                { "closing", closing }
            };
        }
    }

    public static class Extensions
    {
        static Regex RegexEncapsulateLiterals { get; } = new Regex(@"(\\[\\n\^\.\[\$\(\)\|\*\+\?\{\\])");
        static Regex RegexEncapsulators { get; } = new Regex(@"[\\n\^\.\[\$\(\)\|\*\+\?\{\\]");
        static Regex RegexMultiWhitespace { get; } = new Regex(@"\s+");
        static Regex RegexMultiChar { get; } = new Regex(@"([\w\W])\1{3,}");
        static Regex RegexDecompress { get; } = new Regex(@"(@(\d+)[#])|(&(\d+)([\w\W]))");

        public static bool Is<T>(this T obj, params T[] args)
        {
            foreach (object item in args)
            {
                try
                {
                    if (item.Equals(obj))
                    {
                        return true;
                    }
                }
                catch
                {
                    if (item == null && obj == null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static string Format(this string text, params object[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                text = Regex.Replace(text, i.Encapsulate(@"\{"), args[i].ToString());
            }

            return text;
        }

        public static string Translate<T>(this string text, string pattern, Dictionary<string, T> dictionary)
        {
            string[] words = text.Split(' ');

            foreach (string word in words)
            {
                if (dictionary.ContainsKey(word))
                {
                    text = Regex.Replace(text, Format(pattern, word), dictionary[word].ToString());
                }
            }

            return text;
        }

        public static string Standardize(this string text, bool processHomophones = false)
        {
            text = text.Trim().ToLower(); // Trim whitespaces and convert string to lower case
            string pattern = @"(\s{0}|{0}\s|\s{0}\s)"; // The pattern to use to Translate()

            text = text.Translate(pattern, Numerals);

            if (processHomophones)
            {
                text = text.Translate(pattern, Homophones);
            }

            return RegexMultiWhitespace.Replace(text, "");
        }

        public static bool IsSynonym(this string s1, string s2, bool processHomophones = false)
        {
            return s1.Standardize(processHomophones) == s2.Standardize() ? true : false;
        }

        public static string Compress(this string text)
        {
            MatchCollection matchCollection = RegexMultiChar.Matches(text);

            foreach(Match match in matchCollection)
            {
                char c = match.Value[0];
                int i = match.Value.Length;
                string compressed;

                if (c == ' ')
                {
                    compressed = "@" + i + "#";
                }
                else
                {
                    compressed = "&" + i + c;
                }

                text = text.Replace(match.Value, compressed);
            }

            return text;
        }

        public static string Decompress(this string text)
        {
            MatchCollection matchCollection = RegexDecompress.Matches(text);

            foreach(Match match in matchCollection)
            {
                char c;
                int i;

                if (match.Value.StartsWith("@"))
                {
                    c = ' ';
                    i = Int32.Parse(match.Groups[2].Value);
                }
                else
                {
                    c = match.Groups[5].Value[0];
                    i = Int32.Parse(match.Groups[4].Value);
                }

                text = text.Replace(match.Value, new string(c, i));
            }

            return text;
        }

        public static string Encapsulate(this object obj, object encapsulator)
        {
            string text = obj.ToString();
            string openingEncapsulator;
            string closingEncapsulator;

            if (encapsulator is Encapsulator encapsulatorType)
            {
                Dictionary<string, char> encapsulators = EncapsulatorDictionary[encapsulatorType];
                openingEncapsulator = encapsulators["opening"].ToString();
                closingEncapsulator = encapsulators["closing"].ToString();
            }
            else
            {
                openingEncapsulator = encapsulator.ToString();
                closingEncapsulator = "";

                if (RegexEncapsulateLiterals.IsMatch(openingEncapsulator))
                {
                    bool checknext = false;

                    foreach (char c in openingEncapsulator)
                    {
                        if (!checknext)
                        {
                            if (c == '\\')
                            {
                                checknext = true;
                            }
                            else
                            {
                                if (Encapsulators.ContainsKey(c))
                                {
                                    closingEncapsulator = Encapsulators[c] + closingEncapsulator;
                                }
                                else
                                {
                                    closingEncapsulator = c + closingEncapsulator;
                                }
                            }
                        }
                        else
                        {
                            if (RegexEncapsulators.IsMatch(c.ToString()))
                            {
                                if (Encapsulators.ContainsKey(c))
                                {
                                    closingEncapsulator = @"\" + Encapsulators[c] + closingEncapsulator;
                                }
                                else
                                {
                                    closingEncapsulator = @"\" + c + closingEncapsulator;
                                }
                            }
                            else
                            {
                                if (Encapsulators.ContainsKey(c))
                                {
                                    closingEncapsulator = Encapsulators[c] + @"\" + closingEncapsulator;
                                }
                                else
                                {
                                    closingEncapsulator = c + @"\" + closingEncapsulator;
                                }
                            }

                            checknext = false;
                        }
                    }
                }
                else
                {
                    foreach (char c in openingEncapsulator)
                    {
                        if (Encapsulators.ContainsKey(c))
                        {
                            closingEncapsulator = Encapsulators[c] + closingEncapsulator;
                        }
                        else
                        {
                            closingEncapsulator = c + closingEncapsulator;
                        }
                    }
                }
            }

            return openingEncapsulator + text + closingEncapsulator;
        }

        public static string Join(this object[] obj, string separator = null)
        {
            string text = "";

            for (int i = 0; i < obj.Length; i++)
            {
                if (i < obj.Length - 1 && separator != null)
                {
                    text += obj[i].ToString() + separator;
                }
                else
                {
                    text += obj[i].ToString();
                }
            }

            return text;
        }

        public static string Join(this object obj, params object[] objArray)
        {
            string text = obj.ToString();

            foreach(object o in objArray)
            {
                text += o.ToString();
            }

            return text;
        }

        public static string AppendLine(this object obj, object append)
        {
            string text = (!IsEmpty(obj)) ? obj.ToString() + Environment.NewLine : "";

            return text + append.ToString(); 
        }

        public static string Insert(this string text, string insert, int index)
        {
            return text.Substring(0, index) + insert + text.Substring(index);
        }

        public static string Multiply(this string text, int by)
        {
            string result = text;

            for(int i = 0; i < by; i++)
            {
                result += text;
            }

            return result;
        }

        public static int Count(this string haystack, string needle)
        {
            return (haystack.Length - haystack.Replace(needle, "").Length) / needle.Length; 
        }

        public static bool IsEmpty<T>(this List<T> list)
        {
            return list.Count == 0;
        }

        public static bool IsEmpty<T>(this T obj)
        {
            return obj.ToString() == "";
        }

        public static bool IsNull<T>(this T obj)
        {
            return obj == null;
        }

        public static int GetLength<T>(this T obj)
        {
            return obj.ToString().Length;
        }
    }

    public static class Methods
    {
        /// <summary>
        /// Randomize a number from zero to max.
        /// </summary>
        /// <param name="max">The maximum value.</param>
        /// <param name="original">An optional value that the method will avoid returning</param>
        /// <returns>Returns an int within the specified range that is not equals to the original value.</returns>
        public static int Randomize(int max, int original = -1)
        {
            var rnd = new Random();
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

        public static string Format(string text, params object[] args)
        {
            return text.Format(args);
        }

        public static void OpenURL(string url)
        {
            // "chrome --start-maximized https://github.com/TsurugiDanzen/Remo" will open Chrome;
            // "microsoft-edge: https://github.com/TsurugiDanzen/Remo" will open Edge

            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        public static List<string> CreateOverrideArgumentList(params string[] args)
        {
            var listIntList = new List<List<int>>() { new List<int>() };

            for (int i = 0; i < args.Length; i++)
            {
                listIntList[0].Add(i);
            }

            bool stopLoop = false;

            while (!stopLoop)
            {
                for (int a = listIntList.Count; a > 0; a--)
                {
                    int b = a - 1;

                    for (int c = 0; c < listIntList[b].Count; c++)
                    {
                        var combo = new List<int>();

                        listIntList[b].Reverse();

                        for (int d = 0; d < listIntList[b].Count; d++)
                        {
                            if (d != c)
                            {
                                combo.Add(listIntList[b][d]);
                            }
                        }

                        combo.Reverse();
                        listIntList[b].Reverse();

                        bool isMatch = false;

                        foreach (List<int> intList in listIntList)
                        {
                            if (intList.Count == combo.Count)
                            {
                                isMatch = true;

                                for (int f = 0; f < combo.Count; f++)
                                {
                                    if (intList[f] != combo[f])
                                    {
                                        isMatch = false;
                                        break;
                                    }
                                }

                                if (isMatch)
                                {
                                    break;
                                }
                            }
                        }

                        if (!isMatch)
                        {
                            if (combo.Count > 0)
                            {
                                listIntList.Add(combo);
                            }
                            else
                            {
                                stopLoop = true;
                            }
                        }
                    }
                }
            }

            var sortedList = new List<List<int>>();

            for (int i = 0; i < args.Length; i++)
            {
                for (int j = 0; j < listIntList.Count; j++)
                {
                    if (listIntList[j][0] == i)
                    {
                        sortedList.Add(listIntList[j]);
                    }
                }
            }

            var stringList = new List<string>();

            foreach (List<int> l in sortedList)
            {
                var s = new List<string>();

                foreach (int i in l)
                {
                    s.Add(args[i]);
                }

                stringList.Add(string.Join(", ", s));
            }

            return stringList;
        }
    }
}
