namespace Konsole
{
    using System;
    using System.Text.RegularExpressions;
    using Global;

    using static Static;

    public partial class Kontext
    {
        public class LogEntry
        {
            public string Name { get; private set; }
            public DateTime Time { get; private set; }
            public OperationMethod Operation { get; private set; }
            public string Text { get; private set; }

            public LogEntry(string name, OperationMethod operation, string text)
            {
                Name = name;
                Time = DateTime.Now;
                Operation = operation;
                Text = Regex.Replace(text, @"[\r\n]+", @"\n");
            }

            public string ToString(string instance, bool truncate = false)
            {
                int nameLength = 0;

                if (instance == null)
                {
                    foreach (string n in Names)
                    {
                        nameLength = (n.Length > nameLength) ? n.Length : nameLength;
                    }
                }
                else
                {
                    nameLength = instance.Length;
                }

                string name = Name.PadRight(nameLength);
                string operation = Operation.ToString().PadRight(9);
                string text = (truncate) ? (Text.Length <= 70) ? Text : Text.Substring(0, 70) + "[...]" : Text;

                return "{0} | {1} | {2} | {3}".Format(Time.ToString("MM/dd/yy HH:mm:ss:fff"), name, operation, text);
            }

            public string ToString(bool truncate = false)
            {
                return ToString(null, truncate);
            }
        }
    }
}
