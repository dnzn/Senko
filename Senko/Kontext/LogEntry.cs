namespace Kontext
{
    using System;
    using System.Text.RegularExpressions;
    using Generic;

    using static Static;
    using static Konsole;

    public class LogEntry
    {
        public string Name { get; private set; }
        public DateTime Time { get; private set; }
        public DateTime Start { get; private set; }
        public string ElapsedTime { get { return (Time - Start).TotalMilliseconds.ToString("0.#0ms"); } }
        public OperationMethod Operation { get; private set; }
        public string Text { get; private set; }

        public LogEntry(Konsole konsole, DateTime start, OperationMethod operation, string text)
        {
            if (konsole != null)
            {
                Name = konsole.Name;
            }
            else
            {
                Name = "Konsole";
            }

            Time = DateTime.Now;
            Start = start;
            Operation = operation;
            Text = Regex.Replace(text, @Environment.NewLine, @"\n");
        }

        public string ToString(string instance, bool truncate = false)
        {
            int nameLength = 0;

            string operation = Operation.ToString().PadRight(9);
            string elapsedTime = ElapsedTime.PadLeft(8);

            if (instance == null)
            {
                foreach (string n in Names)
                {
                    nameLength = (n.Length > nameLength) ? n.Length : nameLength;
                }

                string name = Name.PadRight(nameLength);

                return "{0} | {1} | {2} | {3} | {4}".Format(Time.ToString("HH:mm:ss:fff"), elapsedTime, name, operation, Text);
            }
            else
            {
                return "{0} | {1} | {2} | {3}".Format(Time.ToString("HH:mm:ss:fff"), elapsedTime, operation, Text);
            }
        }

        public string ToString(bool truncate = false)
        {
            return ToString(null, truncate);
        }
    }
}
