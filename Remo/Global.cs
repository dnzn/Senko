using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Remo
{
    public static class Global
    {
        public static readonly string[] Numbers = new string[]
        {
            "zero", "one", "two", "three", "four",
            "five", "six", "seven", "eight", "nine"
        };

        public static readonly string[][] Homophones = new string[][]
        {
            new string[] { "zero" },
            new string[] { "one", "won" },
            new string[] { "two", "to", "too" },
            new string[] { "three", "tree" },
            new string[] { "four", "for" },
            new string[] { "five" },
            new string[] { "six"},
            new string[] { "seven"},
            new string[] { "eight", "ate"},
            new string[] { "nine"}
        };

        public static bool Is<T>(this T obj, params T[] args)
        {
            foreach (object item in args) { if (item.Equals(obj)) { return true; } } return false;
        }

        /// <summary>
        /// Remove whitespaces, convert text to lower case, and convert numerical words to numbers.
        /// </summary>
        /// <param name="text">The string to be modified.</param>
        /// <returns></returns>
        public static string Standardize(this string text)
        {
            for (int i = 0; i < Numbers.Length; i++)
            {
                text = Regex.Replace(text, Numbers[i], i.ToString());
            }

            return Regex.Replace(text, @"\s+", "").ToLower();
        }

        /// <summary>
        /// Compares two strings to determine whether they are synonymous or equal in context.
        /// </summary>
        /// <param name="s1">The first string</param>
        /// <param name="s2">The second string</param>
        /// <returns></returns>
        public static bool IsSynonym(this string s1, string s2)
        {
            return s1.Standardize() == s2.Standardize() ? true : false;
        }
    }
}
