namespace Remo
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using System.Linq;

    public static class Global
    {
        public static Konsole Kon = new Konsole();

        public static Konsole Log = new Konsole();

        public static readonly Dictionary<string, int> Numbers = new Dictionary<string, int>
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

        public static Dictionary<string, object> Homophones = new Dictionary<string, object>
        {
            { "won", 1 },
            { "to", 2 },
            { "too", 2 },
            { "tree", 3 },
            { "for", 4 },
            { "ate", 8 },
        };

        public static readonly Dictionary<char, char> Encapsulators = new Dictionary<char, char>
        {
            { '(', ')' },
            { '[', ']' },
            { '{', '}' },
            { '<', '>' }
        };

        public class JSON
        {
            public string Raw { get; private set; }

            public List<Dictionary<string, string>> Lexicon { get; private set; }

            public JSON(string json)
            {
                Lexicon = new List<Dictionary<string, string>>();
                Raw = json;

                SonyParse(Raw);
            }

            internal void SonyParse(string json)
            {
                json = Regex.Replace(Regex.Match(json, "(\"result\":\\[.+\\])").Value, "(\"result\":\\[)|(\\])", "");

                Parse(json);
            }

            internal void Parse(string json)
            {
                foreach (string r in Regex.Split(json, "\\}\\s*,\\s*\\{"))
                {
                    string result = r.Trim('{').Trim('}').Replace(@"\/", "/");

                    Dictionary<string, string> entry = new Dictionary<string, string>();

                    foreach (Match matches in Regex.Matches(result, "(\"\\w+\"\\s*:\\s*(\"([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})\"|\"[0-9A-Za-z-.\\/:+@_=\\s]+\"|\\d+))"))
                    {
                        string[] KeyValue = Regex.Split(matches.Value, "\"\\s*:\\s*\"");

                        for (int j = 0; j < KeyValue.Length; j++)
                        {
                            KeyValue[j] = KeyValue[j].Trim().Trim('\"');
                        }

                        Kon.WriteLine(KeyValue[0] + " : " + KeyValue[1]);

                        entry.Add(KeyValue[0], KeyValue[1]);
                    }

                    Lexicon.Add(entry);
                }
            }
        }

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

        public static string Standardize(this string text, bool processHomophones = false)
        {
            text = text.Trim().ToLower(); // Trim whitespaces and convert string to lower case
            string pattern = @"(\s{0}|{0}\s|\s{0}\s)"; // The pattern to use to Translate()

            text = text.Translate(pattern, Numbers);

            if (processHomophones)
            {
                text = text.Translate(pattern, Homophones);
            }

            return Regex.Replace(text, @"\s+", "");
        }

        public static bool IsSynonym(this string s1, string s2, bool processHomophones = false)
        {
            return s1.Standardize(processHomophones) == s2.Standardize() ? true : false;
        }

        public static string Format(this string text, params object[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                text = Regex.Replace(text, i.Encapsulate(@"(\{"), args[i].ToString());
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

        public static string Encapsulate(this object obj, object opening)
        {
            string text = obj.ToString();
            string closing = "";

            if (Regex.IsMatch(opening.ToString(), @"(\\[\\n\^\.\[\$\(\)\|\*\+\?\{\\])"))
            {
                bool checknext = false;

                foreach (char c in opening.ToString())
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
                                closing = Encapsulators[c] + closing;
                            }
                            else
                            {
                                closing = c + closing;
                            }
                        }
                    }
                    else
                    {
                        if (Regex.IsMatch(c.ToString(), @"[\\n\^\.\[\$\(\)\|\*\+\?\{\\]"))
                        {
                            if (Encapsulators.ContainsKey(c))
                            {
                                closing = @"\" + Encapsulators[c] + closing;
                            }
                            else
                            {
                                closing = @"\" + c + closing;
                            }
                        }
                        else
                        {
                            if (Encapsulators.ContainsKey(c))
                            {
                                closing = Encapsulators[c] + @"\" + closing;
                            }
                            else
                            {
                                closing = c + @"\" + closing;
                            }
                        }

                        checknext = false;
                    }
                }
            }
            else
            {
                foreach (char c in opening.ToString())
                {
                    if (Encapsulators.ContainsKey(c))
                    {
                        closing = Encapsulators[c] + closing;
                    }
                    else
                    {
                        closing = c + closing;
                    }
                }
            }

            return opening + text + closing;
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
            List<List<int>> listIntList = new List<List<int>>() { new List<int>() };

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
                        List<int> combo = new List<int>();

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

            List<List<int>> sortedList = new List<List<int>>();

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

            List<string> stringList = new List<string>();

            foreach (List<int> l in sortedList)
            {
                List<string> s = new List<string>();

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
