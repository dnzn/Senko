namespace Polymer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using static Static;
    using static Extensions;
    using static Konsole;
    using static Konsole.Parameters;
    using static Polymer;

    public static partial class Polymer
    {
        public static void WriteLog(this Konsole konsole, params WriteLogParameters[] parameters)
        {
            Konsole.WriteLog(konsole, parameters);
        }

        public static void WriteLog(this Konsole konsole)
        {
            WriteLog(konsole, WriteLogParameters.WriteToConsole, WriteLogParameters.WriteToFile, WriteLogParameters.Truncate);
        }

        public static bool Contains(this List<Konsole> konsoles, string name)
        {
            foreach (Konsole konsole in konsoles)
            {
                if (konsole.Name == name)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public partial class Konsole
    {
        public static Konsole MainKonsole { get; set; }

        static Konsole _MainInfoKonsole;
        public static Konsole MainInfoKonsole
        {
            get => _MainInfoKonsole;
            set
            {
                _MainInfoKonsole = value;

                MainInfoKonsole.Prefix.Prompt = " ";
                MainInfoKonsole.Prefix.CurrentPrefix = PrefixType.Indent;
                MainInfoKonsole.Color.PrimaryColor = ConsoleColor.Cyan;
                MainInfoKonsole.Color.SecondaryColor = ConsoleColor.DarkCyan;
            }
        }
        public static Konsole MainErrorKonsole { get; set; }
        public static List<Konsole> Konsoles { get; } = new List<Konsole>();

        public enum OperationMethod
        {
            Write,
            WriteLine,
            Read,
            ReadLine,
            WriteLog,
            PreviousOperation
        };

        public enum WriteParameters
        {
            /// <summary>
            /// A log entry will not be created.
            /// </summary>
            OffTheRecord,
            /// <summary>
            /// Colors and other tags will not be processed. Any color, prefix, and newline arguments will be ignored.
            /// </summary>
            MinimalProcessing,
            ColorSplitterRandomSort,
            ColorSplitterCountWhiteSpaces
        }

        public enum WriteLogParameters
        {
            WriteToConsole,
            WriteToFile,
            Truncate
        };

        public string Name { get; private set; }
        static int CursorLeft { get => Console.CursorLeft; set => Console.CursorLeft = value; }
        static int WindowWidth { get => Console.WindowWidth; }
        
        public Konsole(string instanceName)
        {
            Name = instanceName;
            Color = new Color(this);
            Prefix = new Prefix(this);
            NewLine = new NewLine(this);
            Console.CursorVisible = false;

            if (Name.Contains("Error") && MainErrorKonsole == null)
            {
                MainErrorKonsole = this;
            }
            else if (Name.Contains("Info") && MainInfoKonsole == null)
            {
                MainInfoKonsole = this;
            }
            else if (MainKonsole == null)
            {
                MainKonsole = this;
            }

            if (!Konsoles.Contains(Name))
            {
                Konsoles.Add(this);
            }

            Log.LoadFromFile();
        }

        void Print(string text, OperationMethod method, params dynamic[] dynamicArgs)
        {
            var writeParameters = new List<WriteParameters>();

            if (method.ToString().Contains("Write"))
            {
                var stringList = new List<string>();
                var colorSplitter = new Color.Splitter();
                var longTextFormatting = LongTextFormatting.WordWrap;
                PrefixType prefix = Prefix.CurrentPrefix;
                NewLineType newline = (method == OperationMethod.Write) ? NewLine.Write : NewLine.WriteLine;
                ConsoleColor color = (Color.ForceColorReset) ? Color.PrimaryColor : Color.ForegroundColor;

                foreach (dynamic arg in dynamicArgs)
                {
                    if (stringList.Count == 0)
                    {
                        if (arg is WriteParameters)
                        {
                            writeParameters.Add(arg); 
                        }
                        else if (arg is LongTextFormatting)
                        {
                            longTextFormatting = arg;
                        }
                        else if (arg is NewLineType)
                        {
                            newline = (method == OperationMethod.Write) ? NewLine.OverrideWrite = arg : NewLine.OverrideWriteLine = arg;
                        }
                        else if (arg is PrefixType)
                        {
                            prefix = Prefix.Override = arg;
                            newline = (method == OperationMethod.Write) ? NewLine.OverrideWrite : NewLine.OverrideWriteLine;
                        }
                        else if (arg is ConsoleColor)
                        {
                            color = arg;
                        }
                        else if (arg is Color.Splitter)
                        {
                            colorSplitter = arg;
                        }
                        else
                        {
                            stringList.Add(arg.ToString());
                        }
                    }
                    else
                    {
                        stringList.Add(arg.ToString());
                    }
                }

                if (!writeParameters.Contains(WriteParameters.OffTheRecord))
                {
                    Log.Initialize(this, method);
                }

                if (writeParameters.Contains(WriteParameters.MinimalProcessing))
                {
                    prefix = PrefixType.None;
                    newline = NewLineType.Both;
                }

                text = (stringList.Count > 0) ? text.Format(stringList.ToArray()) : text;

                if (!writeParameters.Contains(WriteParameters.MinimalProcessing))
                {
                    text = colorSplitter.Execute(text, this);
                    text = NewLine.FormatLongText(text, longTextFormatting, Prefix.GetPrefix(prefix));
                }

                if (prefix == Prefix.CurrentPrefix)
                {
                    if (CursorLeft == 0 || (CursorLeft > 0 && (newline == NewLineType.Prepend || newline == NewLineType.Both)))
                    {
                        text = Prefix.Insert(text);
                    }
                }
                else
                {
                    text = Prefix.Insert(text, prefix);
                }

                text = NewLine.Insert(text, newline);

                if (!writeParameters.Contains(WriteParameters.MinimalProcessing))
                {
                    Printer(text, this, color, !writeParameters.Contains(WriteParameters.OffTheRecord));
                }
                else
                {
                    Printer(text, !writeParameters.Contains(WriteParameters.OffTheRecord));
                }

                Color.Reset();
            }
        }

        static void Printer(string text, bool writeToLog = true)
        {
            Console.Write(text);

            if (writeToLog)
            {
                Log.Commit(text);
            }
        }

        static void Printer(string text, Konsole konsole, ConsoleColor color, bool writeToLog = true)
        {
            string[] textArray = Color.Expand(text);

            for (int i = 0; i < textArray.Length; i++)
            {
                if (color != Color.ForegroundColor && !Color.StartsWithTag(textArray[i], konsole))
                {
                    textArray[i] = textArray[i].InsertTag(color);
                }

                Console.Write(Color.Paint(textArray[i], konsole));

                if (writeToLog)
                {
                    if (i == 0)
                    {
                        Log.Commit(textArray[i]);
                    }
                    else
                    {
                        Log.Commit(OperationMethod.PreviousOperation, textArray[i]);
                    }
                }
            }
        }

        static void Printer(string text, Konsole konsole, bool writeline = true)
        {
            Printer(Color.Expand(text), konsole, writeline);
        }

        static void Printer(IEnumerable<string> stringEnumerable, Konsole konsole, bool writeline = true)
        {
            foreach(string text in stringEnumerable)
            {
                Console.Write(Color.Paint(text, konsole));
            }

            if (writeline)
            {
                Console.WriteLine();
            }
        }

        public void Write(object obj, params object[] objectArray)
        {
            Print(obj.ToString(), OperationMethod.Write, objectArray);
        }

        public void Write(object[] obj, params object[] objectArray)
        {
            Write(obj.Join(), objectArray);
        }

        public void WriteLine(object obj, params object[] objectArray)
        {
            Print(obj.ToString(), OperationMethod.WriteLine, objectArray);
        }

        public void WriteLine()
        {
            Console.WriteLine();
        }

        static string HorizontalDivider(char c)
        {
            return " " + new string(c, NewLine.MaxLength());
        }

        public static void WriteLog(Konsole konsole, params WriteLogParameters[] parameters)
        {
            Console.ResetColor();
            Console.WriteLine();
            
            if (parameters.Contains(WriteLogParameters.WriteToConsole))
            {
                Log.Initialize(konsole, OperationMethod.WriteLog);

                Konsole printer;
                List<Log.Record> records;
                List<string> processedRecords = new List<string>();
                Log.MaxLengths maxLengths;

                if (konsole != null)
                {
                    printer = konsole;
                    records = Log.FilterRecords(konsole);
                    maxLengths = new Log.MaxLengths(records);

                    Console.WriteLine(" Log entries for a specific instance will be displayed.\n Be aware that the process IDs (first column) will not be in sequence.");
                }
                else
                {
                    printer = MainKonsole;
                    records = Log.Records;
                    maxLengths = Log.MaxLength;
                }

                Console.WriteLine(" Processing log information. This might take a while. Please wait...");

                ConsoleColor tableColor = ConsoleColor.DarkGray;

                string thickDivider = HorizontalDivider('=');
                string thinDivider = HorizontalDivider('-');

                string headerText = "[KONSOLE LOG] : " + ((konsole != null) ? konsole.Name : "Full Record");
                string logStart = "LOG START";

                processedRecords.Add(thickDivider, ConsoleColor.DarkCyan, NewLineType.Append);
                processedRecords.Add(headerText.ApplyIndent(1) + new string(' ', printer.MaxLength(PrefixType.Normal) - headerText.Length - logStart.Length), ConsoleColor.Cyan);
                processedRecords.Add(logStart, ConsoleColor.DarkCyan,NewLineType.Append);
                processedRecords.Add(thickDivider, ConsoleColor.DarkCyan, NewLineType.Append);

                string lastDate = "";
                int dateChanged = 0;

                foreach (Log.Record record in records)
                {
                    string row;

                    string logDate = record.StartTime.ToString("dddd, MMMM d, yyyy");
                    int logCountLength = records.Count.GetLength();
                    string processID = record.ProcessID.ToString(new string('#', logCountLength)).PadLeft(logCountLength);
                    string time = record.StartTime.ToString("HH:mm:ss:fff");
                    string elapsedTime = record.ElapsedTime.PadLeft(maxLengths.ElapsedTimes);
                    string operation = record.Operation.PadRight(maxLengths.Operations);
                    string text = (record.Operation == OperationMethod.WriteLog.ToString()) ? record.Text.Encapsulate(EncapsulatorType.Brackets) : record.Text;
                    string colorTag = Color.CreateTag((record.Operation != OperationMethod.PreviousOperation.ToString()) ? ConsoleColor.White : ConsoleColor.Gray);
                    string divider = Color.InsertTag(" | ", @"\|", tableColor, colorTag);
                    string operationID;
                    string iterationID;
                    string name;
                    int idLength = maxLengths.OperationIDs + maxLengths.IterationIDs + 1;

                    if (record.Operation != "")
                    {
                        operationID = record.OperationID.ToString(new string('#', maxLengths.OperationIDs)).PadLeft(maxLengths.OperationIDs);
                        iterationID = record.IterationID.ToString(new string('#', maxLengths.IterationIDs)).PadLeft(maxLengths.IterationIDs);
                        name = (record.Name != "") ? record.Name.PadRight(maxLengths.Names) : new string('-', maxLengths.Names);
                    }
                    else
                    {
                        operationID = new string(' ', maxLengths.OperationIDs);
                        iterationID = record.IterationID.ToString(new string('#', maxLengths.IterationIDs).PadLeft(maxLengths.IterationIDs));
                        name = new string(' ', maxLengths.Names);
                    }

                    bool dateDivider = false;

                    if (lastDate != logDate)
                    {
                        if (dateChanged > 0)
                        {
                            processedRecords.Add(thickDivider, ConsoleColor.DarkCyan, NewLineType.Append);
                        }

                        dateChanged++;
                        processedRecords.Add(logDate.ApplyIndent(1), ConsoleColor.Cyan, NewLineType.Append);                        
                        processedRecords.Add(thinDivider, ConsoleColor.DarkCyan, NewLineType.Append);

                        string label;

                        lastDate = logDate;

                        string nameLabel = ((name.Length >= (label = "NAME").Length) ? label : "N").PadRight(name.Length);
                        string processIDLabel = ((processID.Length >= (label = "PID").Length) ? label : "ID").PadRight(processID.Length);
                        string timeLabel = "TIME".PadRight(time.Length);
                        string elapsedTimeLabel = ((elapsedTime.Length >= (label = "ELAPSED").Length) ? label : "ET").PadRight(elapsedTime.Length);
                        string operationLabel = ((operation.Length >= (label = "METHOD").Length) ? label : "M").PadRight(operation.Length);
                        string operationIDLabel = ((operationID.Length >= (label = "OID").Length) ? label : "O").PadRight(operationID.Length);
                        string iterationIDLabel = "#".PadRight(iterationID.Length);
                        string textLabel = "TEXT";

                        List<string> labels = (konsole == null) ?
                            new List<string>() { processIDLabel, timeLabel, elapsedTimeLabel, operationLabel, operationIDLabel, iterationIDLabel, textLabel } :
                            new List<string>() { timeLabel, elapsedTimeLabel, operationLabel, operationIDLabel, iterationIDLabel, textLabel };

                        row = ((konsole == null) ? nameLabel : processIDLabel).ApplyIndent(1);

                        for (int j = 0; j < labels.Count; j++)
                        {
                            row += Color.CleanTags(divider) + labels[j];
                        }

                        row = Color.CreateTag(ConsoleColor.DarkCyan) + row;
                        processedRecords.AddRange(Color.Expand(row + Environment.NewLine));

                        dateDivider = true;
                    }

                    if (record.Operation != "" && !dateDivider)
                    {
                        processedRecords.Add(thinDivider, tableColor, NewLineType.Append);
                    }
                    else if (dateDivider)
                    {
                        processedRecords.Add(thickDivider, ConsoleColor.DarkCyan, NewLineType.Append);
                    }

                    if (konsole == null || konsole.Name == record.Name)
                    {
                        List<string> list = (konsole == null) ?
                            new List<string>() { processID, time, elapsedTime, operation, operationID, iterationID, Color.DisableTags(text, konsole) } :
                            new List<string>() { time, elapsedTime, operation, operationID, iterationID, Color.DisableTags(text, konsole) };

                        row = ((konsole == null) ? name : processID).ApplyIndent(1);

                        for (int j = 0; j < list.Count; j++)
                        {
                            row += divider + list[j];                         
                        }

                        if (parameters.Contains(WriteLogParameters.Truncate) && Color.CleanTags(row, konsole).Length > WindowWidth)
                        {
                            row = NewLine.Truncate(row);
                        }

                        row = colorTag + Color.InsertTag(row, @"\\n|<\\\w+>", tableColor, colorTag);
                        processedRecords.AddRange(Color.Expand(row + ((record != records.Last()) ? Environment.NewLine : "")));
                    }
                }

                Console.WriteLine();
                Printer(processedRecords, printer);

                int processes = records.Count;
                int operations = Log.CountOperations(records);
                string processPluralize = (processes > 1) ? "processes" : "process";
                string operationPluralize = (operations > 1) ? "operations" : "operation";

                Log.Commit(processes + ((processes > 1) ? " log entries were written..." : " log entry was written..."));
                
                string processTime = " WriteLog completed in {0} for {1} {2} and {3} {4}.".Format(Log.Records.Last().ElapsedTime.Trim(), processes, processPluralize, operations, operationPluralize);
                string logEnd = "LOG END";
                string footer = Color.CreateTag(ConsoleColor.DarkCyan) + thinDivider;
                footer = footer.AppendLine(processTime + new string(' ', WindowWidth - logEnd.Length - processTime.Length - 1) + logEnd, ConsoleColor.DarkCyan);
                footer = footer.AppendLine(thickDivider, ConsoleColor.DarkCyan);

                Printer(footer, printer);
            }

            if (parameters.Contains(WriteLogParameters.WriteToFile))
            {
                Log.SaveToFile();
            }
        }

        public static void WriteLog()
        {
            WriteLog(null, WriteLogParameters.WriteToConsole, WriteLogParameters.WriteToFile, WriteLogParameters.Truncate);
        }

        public static void WriteLog(params WriteLogParameters[] parameters)
        {
            WriteLog(null, parameters);
        }
    }
}
