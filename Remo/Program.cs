using System;
using static Remo.Konsole;

namespace Remo
{
    class Program
    {
        static void Main(string[] args)
        {
            Konsole Kon = new Konsole(ConsoleColor.Green);

            string s1 = "video 3";
            string s2 = "Video3";

            if (s1.IsSynonym(s2))
            {
                Kon.WriteLine("Yes, {0} is synonymous with {1}", s1, s2);
            }
            else
            {
                Kon.WriteLine("No, {0} is not synonymous with {1}", s1, s2);
            }
            
            Console.ReadLine();
        }
    }
}
