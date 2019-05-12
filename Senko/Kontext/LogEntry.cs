namespace Kontext
{
    using System;
    using System.Text.RegularExpressions;
    using Global;

    using static Static;
    using static Konsole;

    public class LogEntry
    {
        public string Name { get; private set; }
        public DateTime Time { get; private set; }
        public DateTime Start { get; private set; }
        public string Operation { get; private set; }
        public string Text { get; private set; }

        public LogEntry(string name, DateTime start, OperationMethod operation, string text)
        {
            Init(name, start, operation, text);
        }

        public LogEntry(string name, DateTime start, string operation, string text)
        {
            Init(name, start, operation, text);
        }

        public LogEntry(string name, DateTime start, string text)
        {
            Init(name, start, "", text);
        }

        public void Init(string name, DateTime start, object operation, string text)
        {
            Name = name;
            Time = DateTime.Now;
            Start = start;
            Operation = operation.ToString();
            Text = Regex.Replace(text, @Environment.NewLine, @"\n");
        }

        public string ToString(string instance, bool truncate = false)
        {
            int nameLength = 0;
            int elapsedTimeLength = 0;

            string operation = Operation.PadRight(9);
            string elapsedTime = (Time - Start).TotalMilliseconds.ToString("0.#0ms").PadLeft(8);

            if (instance == null)
            {
                foreach (string n in Names)
                {
                    nameLength = (n.Length > nameLength) ? n.Length : nameLength;
                }

                int l = 71 - nameLength;
                string text = (truncate) ? (Text.Length <= l) ? Text : Text.Substring(0, l) + "[...]" : Text;
                string name = Name.PadRight(nameLength);

                return "{0} | {1} | {2} | {3} | {4}".Format(Time.ToString("HH:mm:ss:fff"), elapsedTime, name, operation, text);
            }
            else
            {
                int l = 74;
                string text = (truncate) ? (Text.Length <= l) ? Text : Text.Substring(0, l) + "[...]" : Text;

                return "{0} | {1} | {2} | {3}".Format(Time.ToString("HH:mm:ss:fff"), elapsedTime, operation, text);
            }
        }

        public string ToString(bool truncate = false)
        {
            return ToString(null, truncate);
        }
    }
}
