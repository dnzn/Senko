namespace Remo
{
    using System;
    using System.Text.RegularExpressions;

    public partial class Konsole
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

            public string ToString()
            {
                return "<{0}> [{1}] {2}: {3}".Format(Name, Time.ToString("MM/dd/yy HH:mm:ss"), Operation, Text);
            }
        }
    }
}
