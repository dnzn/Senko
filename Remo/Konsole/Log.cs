namespace Remo
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;

    public partial class Konsole
    {
        public class LogEntry
        {
            public enum OperationMethod { Write, WriteLine, Read, ReadLine };

            public string Name { get; private set; }
            public DateTime Time { get; private set; }
            public OperationMethod Operation { get; private set; }
            public string Text { get; private set; }

            public LogEntry(string name, OperationMethod operation, string text)
            {
                Name = name;
                Time = DateTime.Now;
                Operation = operation;
                Text = Regex.Replace(text, Environment.NewLine, @"\n");
            }
        }
    }
}
