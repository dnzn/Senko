using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static Remo.Konsole;

namespace Remo
{
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

        /// <summary>
        /// This one was created for the Google Assistant and IFTTT integration.
        /// Sometimes, text-to-speech just doesn't work well so this will help catch possible mistakes.
        /// </summary>
        public static readonly Dictionary<string, object> Homophones = new Dictionary<string, object>
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

        /// <summary>
        /// A JSON class that I whipped on my own
        /// It takes a string and gets all key-value pairs and set them on a dictionary (Lexicon) for easy searching
        /// </summary>
        public class JSON
        {
            public string Raw { get; private set; }
            public Dictionary<string, string> Lexicon { get; private set; }
            
            public JSON(string json)
            {
                Lexicon = new Dictionary<string, string>();
                Raw = json;

                Parse(Raw);
            }

            void Parse(string json)
            {
                foreach (Match matches in Regex.Matches(json, "(\"\\w+\"\\s*:\\s*(\"([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})\"|\"[0-9A-Za-z-./:+@_\\s]+\"|\\d+))"))
                {
                    string[] KeyValue = matches.Value.Split(':');

                    for (int i = 0; i < KeyValue.Length; i++)
                    {
                        KeyValue[i] = KeyValue[i].Trim().Trim('\"');
                    }
                    
                    Lexicon.Add(KeyValue[0], KeyValue[1]);                    
                }
            }
        }

        /// <summary>
        /// Compare an object with any number of other objects and check if they are equal.
        /// </summary>
        /// <typeparam name="T">Any type that can be compared</typeparam>
        /// <param name="obj">The main object to compare</param>
        /// <param name="args">Other objects to compare with the main object</param>
        /// <returns>True if obj is equal to any of the args</returns>
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

        /// <summary>
        /// Remove whitespaces, convert text to lower case, and convert numerical words to numbers.
        /// </summary>
        /// <param name="text">The string to be modified.</param>
        /// <param name="processHomophones">Set true to process homophones</param>
        /// <returns></returns>
        public static string Standardize(this string text, bool processHomophones = false)
        {
            text = text.ToLower(); // Convert string to lower case
            string pattern = @"(\s{0}|{0}\s|\s{0}\s)"; // The pattern to use to Translate()

            text = text.Translate(pattern, Numbers);

            if (processHomophones)
            {
                text = text.Translate(pattern, Homophones);
            }

            return Regex.Replace(text, @"\s+", "");
        }

        /// <summary>
        /// Compares two, possibly different, strings to determine whether they are synonymous or equal in context.
        /// </summary>
        /// <param name="s1">The first string</param>
        /// <param name="s2">The second string</param>
        /// <returns></returns>
        public static bool IsSynonym(this string s1, string s2, bool processHomophones = false)
        {
            return s1.Standardize(processHomophones) == s2.Standardize() ? true : false;
        }

        //
        public static string Format(this string text, params object[] args)
        {
            if (text != null)
            {
                text = string.Format(text, args);
            }

            return text;
        }

        /// <summary>
        /// Translates the input text to an entry in the dictionary if found. This uses Regex to do the replacements.
        /// </summary>
        /// <typeparam name="T">This should work with any type that can be converted into a string</typeparam>
        /// <param name="text">The text to translate</param>
        /// <param name="pattern">The Regex patterm to use</param>
        /// <param name="dictionary">The dictionary to use</param>
        /// <returns>The translated text. If no translations are available, it will return the original.</returns>
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

        /// <summary>
        /// Encapsulates a string or object in a Regex-pattern-safe manner. The proper parentheses and other brackets are automagically parsed from the capsule.
        /// For example to get this regex pattern (\{[abc]\}), the capsule must only contain @"(\{[" and the method will do the rest.
        /// </summary>
        /// <param name="obj">The object to encapsulate</param>
        /// <param name="capsule"></param>
        /// <returns></returns>
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

        /// <summary>
        /// A hack to open URLs to the default browser using .NET Core.
        /// I found it here: https://brockallen.com/2016/09/24/process-start-for-urls-on-net-core/
        /// One could also start a different browser, if they wish
        /// </summary>
        /// <param name="url">The url to open</param>
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

        /// <summary>
        /// This creates lines of strings that contain all possible method argument override combinations as per the given arguments.
        /// I made this because it can be hard to keep up with methods that could need all possible overrides (like Write).
        /// This isn't particularly used in the program but I may keep it here until it's not needed anymore.
        /// </summary>
        /// <param name="args">The argument names for the method</param>
        /// <returns>A string representation of all possibilities</returns>
        public static List<string> CreateOverrideArgumentList (params string[] args)
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
