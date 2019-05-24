namespace Kontext
{
    using System;
    using System.Text.RegularExpressions;
    using System.Collections.Generic;
    using System.Linq;
    using Generic;

    using static Konsole;
    using static Kontext;
    using static Generic.Fields;

    public static class Log
    {
        public class Record : ICloneable
        {
            public Konsole Instance { get; private set; } = null;
            public string Name { get { return (Instance != null) ? Instance.Name : ""; } }

            public OperationMethod Operation { get; set; }
            public string OperationString { get { return (Operation != OperationMethod.PreviousOperation) ? Operation.ToString() : ""; } }

            public int ProcessID { get; private set; }
            public int OperationID { get; private set; }
            public int IterationID { get; private set; }

            public DateTime StartTime { get; private set; }
            public DateTime EndTime { get; private set; }
            public string ElapsedTime
            {
                get
                {
                    double ms = (EndTime - StartTime).TotalMilliseconds;
                    int divisor;
                    string unit;
                    string elapsedTime;

                    if (ms >= 1)
                    {
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

                        elapsedTime = (ms / divisor).ToString("0.#0") + unit;
                    }
                    else
                    {
                        elapsedTime = (ms * 1000).ToString("0.#0") + "µs";
                    }

                    return elapsedTime;
                }
            }
            
            public string Text { get; private set; }

            public Record(Konsole konsole, OperationMethod operation)
            {                
                Instance = konsole;
                StartTime = DateTime.Now;

                Operation = operation;
            }

            public void Commit(string text)
            {
                SetID();

                Text = text.Replace(@Environment.NewLine, @"\n");
                EndTime = DateTime.Now;
                Records.Add(CurrentRecord);
                MaxLength.Parse(this);
            }

            public void Commit(OperationMethod operation, string text)
            {
                Operation = operation;
                Commit(text);
            }

            void SetID()
            {
                int previousProcessID = (Records.IsEmpty()) ? 0 : Records.Last().ProcessID;
                int previousOperationID = (Records.IsEmpty()) ? 0 : Records.Last().OperationID;
                int previousIterationID = (Records.IsEmpty()) ? 0 : Records.Last().IterationID;

                ProcessID = previousProcessID + 1;

                if (Operation != OperationMethod.PreviousOperation)
                {
                    OperationID = previousOperationID + 1;
                    IterationID = 1;
                }
                else
                {
                    OperationID = previousOperationID;
                    IterationID = previousIterationID + 1;
                }
            }

            public object Clone()
            {
                return new Record(Instance, Operation);
            }
        }

        public class MaxLengths
        {
            public int Name { get; private set; } = 0;
            public int Operation { get; private set; } = 0;
            public int ProcessID { get; private set; } = 0;
            public int OperationID { get; private set; } = 0;
            public int IterationID { get; private set; } = 0;
            public int ElapsedTime { get; private set; } = 0;
            public int Text { get; private set; } = 0;

            List<Record> Records { get; set; }

            public MaxLengths(List<Record> records)
            {
                Records = records;

                if (!Records.IsEmpty())
                {
                    Refresh();
                }
            }

            public void Parse(Record record)
            {
                Name = Math.Max(record.Name.Length, Name);
                Operation = Math.Max(record.OperationString.Length, Operation);
                ProcessID = Math.Max(record.ProcessID.ToString().Length, ProcessID);
                OperationID = Math.Max(record.OperationID.ToString().Length, OperationID);
                IterationID = Math.Max(record.IterationID.ToString().Length, IterationID);
                ElapsedTime = Math.Max(record.ElapsedTime.Length, ElapsedTime);
                Text = Math.Max(record.Text.Length, Text);
            }

            public void Refresh()
            {
                if (!Records.IsEmpty())
                {
                    foreach (Record record in Records)
                    {
                        Parse(record);
                    }
                }
            }
        }

        public static List<Record> Records { get; } = new List<Record>();
        public static int Count { get { return Records.Count; } }
        public static int CountOperations { get { return (Records.Count > 0) ? Records[Records.Count - 1].OperationID : 0; } }

        public static MaxLengths MaxLength { get; set; } = new MaxLengths(Records);

        static Record CurrentRecord { get; set; }

        public static void Initialize(Konsole konsole, OperationMethod operation)
        {
            CurrentRecord = new Record(konsole, operation);
        }

        public static void Initialize(OperationMethod operation)
        {
            Initialize(null, operation);
        }

        public static void Commit(string text)
        {
            if (CurrentRecord != null)
            {
                CurrentRecord.Commit(text);
            }
        }

        public static void Commit(OperationMethod method, string text)
        {
            if (CurrentRecord != null)
            {
                CurrentRecord = (Record)CurrentRecord.Clone();

                CurrentRecord.Commit(method, text);
            }
        }

        public static List<Record> FilterRecords(Konsole konsole)
        {
            var records = new List<Record>();

            if (Records.Count > 0)
            {
                foreach (Record record in Records)
                {
                    if (record.Name == konsole.Name)
                    {
                        records.Add(record);
                    }
                }
            }

            return records;
        }
    }
}
