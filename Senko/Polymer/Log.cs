namespace Polymer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using static Konsole;

    public static class Log
    {
        [Serializable()]
        public class Record
        {
            public string Name { get; set; }
            public string Operation { get; set; }
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
                Initialize((konsole != null) ? konsole.Name : "", operation);               
            }

            public Record(string name, OperationMethod operation)
            {
                Initialize(name, operation);
            }

            void Initialize(string name, OperationMethod operation)
            {
                Name = name;
                StartTime = DateTime.Now;
                Operation = (operation != OperationMethod.PreviousOperation) ? operation.ToString() : "";
            }

            public void Commit(string text)
            {
                SetID();

                Text = text.Replace(@Environment.NewLine, @"\n");
                EndTime = DateTime.Now;
                Records.Add(CurrentRecord, MaxLength);
            }

            int SetID(int id)
            {
                return (Records.IsEmpty()) ? 0 : id;
            }

            void SetID()
            {
                int previousProcessID = SetID(Records.Last().ProcessID);
                int previousOperationID = SetID(Records.Last().OperationID);
                int previousIterationID = SetID(Records.Last().IterationID);

                ProcessID = previousProcessID + 1;

                if (Operation != "")
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

            public Record InitSubProcess(OperationMethod operation)
            {
                return new Record(Name, operation);
            }
        }

        [Serializable()]
        public class MaxLengths
        {
            public int Names { get; private set; } = 0;
            public int Operations { get; private set; } = 0;
            public int ProcessIDs { get; private set; } = 0;
            public int OperationIDs { get; private set; } = 0;
            public int IterationIDs { get; private set; } = 0;
            public int ElapsedTimes { get; private set; } = 0;
            public int Texts { get; private set; } = 0;
            
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
                Names = Math.Max(record.Name.GetLength(), Names);
                Operations = Math.Max(record.Operation.Length, Operations);
                ProcessIDs = Math.Max(record.ProcessID.GetLength(), ProcessIDs);
                OperationIDs = Math.Max(record.OperationID.GetLength(), OperationIDs);
                IterationIDs = Math.Max(record.IterationID.GetLength(), IterationIDs);
                ElapsedTimes = Math.Max(record.ElapsedTime.Length, ElapsedTimes);
                Texts = Math.Max(Parameters.Color.CleanTags(record.Text).Length, Texts);
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

        [Serializable()]
        struct LogFile
        {
            public List<Record> Records;
            public MaxLengths MaxLength;

            public LogFile(List<Record> records, MaxLengths maxLength)
            {
                Records = records;
                MaxLength = maxLength;
            }
        }

        public static List<Record> Records { get; private set; } = new List<Record>();

        public static int Count { get { return Records.Count; } }

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
                CurrentRecord = CurrentRecord.InitSubProcess(method);

                CurrentRecord.Commit(text);
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

        public static int CountOperations(List<Record> records)
        {
            return (records.Count > 0) ? records[records.Count - 1].OperationID : 0;
        }

        public static void SaveToFile(string filename = "")
        {
            LogFile file = new LogFile(Records, MaxLength);

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(@"C:\Users\Danzen Binos\OneDrive\Senko\test.log", FileMode.Create, FileAccess.Write);
            formatter.Serialize(stream, file);
            stream.Close();
        }

        public static void LoadFromFile()
        {
            if (File.Exists(@"C:\Users\Danzen Binos\OneDrive\Senko\test.log"))
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(@"C:\Users\Danzen Binos\OneDrive\Senko\test.log", FileMode.Open, FileAccess.Read);
                LogFile file = (LogFile)formatter.Deserialize(stream);
                stream.Close();

                Records = file.Records;
                MaxLength = file.MaxLength;
            }
        }

        public static void ResetLog()
        {

        }

        public static void DeleteLogFile()
        {

        }

        static void Add(this List<Record> records, Record record, MaxLengths maxLengths)
        {
            maxLengths.Parse(record);
            records.Add(record);
        }
    }
}
