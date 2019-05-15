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
        public string ElapsedTime {
            get
            {
                double ms = (Time - Start).TotalMilliseconds;
                int divisor;
                string unit;

                if (ms >= 60000)
                {
                    divisor = 60000;
                    unit = "m ";
                }
                else if (ms >= 1000)
                {
                    divisor = 1000;
                    unit = "s ";
                }
                else
                {
                    divisor = 1;
                    unit = "ms";
                }

                string elapsedTime = (ms / divisor).ToString("0.#0") + unit;

                return elapsedTime;
            }
        }

        public OperationMethod Operation { get; private set; }
        public string Text { get; private set; }

        public static int NameMaxLength { get; private set; } = 0;
        public static int ElapsedMaxLength { get; private set; } = 0;
        public static int OperationMaxLength { get; private set; } = 0;
        public static int TextMaxLength { get; private set; } = 0;
        public static int Records { get; private set; } = 0;
        public static int SubRecords { get; private set; } = 0;

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

            NameMaxLength = (Name.Length > NameMaxLength) ? Name.Length : NameMaxLength;
            ElapsedMaxLength = (ElapsedTime.Length > ElapsedMaxLength) ? ElapsedTime.Length : ElapsedMaxLength;
            OperationMaxLength = (Operation.ToString().Length > OperationMaxLength) ? Operation.ToString().Length : OperationMaxLength;
            TextMaxLength = (Parameters.Color.CleanTags(Text).Length > TextMaxLength) ? Parameters.Color.CleanTags(Text).Length : TextMaxLength;

            if (Operation != OperationMethod.Previous)
            {
                Records++;
            }
            else
            {
                SubRecords++;
            }
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
