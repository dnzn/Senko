using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace Remo
{
    public static class Global
    {
        public static readonly Dictionary<string, int> Numbers = new Dictionary<string, int>
        {
            { "zero", 0 }, { "one", 1 },  { "two", 2 }, { "three", 3 }, { "four", 4 }, { "five", 5 }, { "six", 6 }, { "seven", 7 }, { "eight", 8 }, { "nine", 9 }
        };

        public static readonly Dictionary<string, object> Homophones = new Dictionary<string, object>
        {
            { "won", 1 }, { "to", 2 }, { "too", 2 }, { "tree", 3 }, { "for", 4 }, { "ate", 8 },
        };

        public static readonly Dictionary<char, char> Encapsulators = new Dictionary<char, char>
        {
            { '(', ')' },
            { '[', ']' },
            { '{', '}' },
            { '<', '>' }
        };
        
        /// <summary>
        /// Compare an object with any number of other objects and check if they are equal.
        /// </summary>
        /// <typeparam name="T">Any generic type</typeparam>
        /// <param name="obj">The main object to compare</param>
        /// <param name="args">Other objects to compare with the main object</param>
        /// <returns></returns>
        public static bool Is<T>(this T obj, params T[] args)
        {
            foreach (object item in args) { if (item.Equals(obj)) { return true; } } return false;
        }

        /// <summary>
        /// Remove whitespaces, convert text to lower case, and convert numerical words to numbers.
        /// </summary>
        /// <param name="text">The string to be modified.</param>
        /// <param name="processHomophones">Set true to process homophones</param>
        /// <returns></returns>
        public static string Standardize(this string text, bool processHomophones = false)
        {
            text = text.ToLower();
            string pattern = @"(\s{0}|{0}\s|\s{0}\s)";

            string[] words = text.Split(' ');

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

        public static string Replace(this string text, params object[] args)
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
                    text = Regex.Replace(text, pattern.Replace(word), dictionary[word].ToString());
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
    }
}
