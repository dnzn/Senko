namespace Polymer
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Generic;
    using System.Linq;

    using static Generic.Fields;
    using static Kontext;
    using static Konsole.Parameters;

    public static class Kontext
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
                MainInfoKonsole.Prefix.Current = Konsole.PrefixType.Indent;
                MainInfoKonsole.Color.PrimaryColor = ConsoleColor.Cyan;
                MainInfoKonsole.Color.SecondaryColor = ConsoleColor.DarkCyan;
            }
        }
        public static Konsole MainErrorKonsole { get; set; }
        public static List<Konsole> Konsoles { get; } = new List<Konsole>();

        public static Regex RegexForceIndent { get; } = new Regex(Environment.NewLine + "|^");

        public static void WriteLog(this Konsole konsole, bool truncate = true)
        {
            Konsole.WriteLog(konsole, Konsole.WriteLogParameters.Truncate);
        }

        public static string ForceIndent(this string text, int indentLength)
        {
            var Indent = new string(' ', indentLength);
            return RegexForceIndent.Replace(text, @"$&" + Indent);
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

        public static void Add(this List<string> list, object obj, ConsoleColor? color, Konsole.NewLineType newLineType = Konsole.NewLineType.None)
        {
            string text = obj.InsertTag(color);

            if (newLineType == Konsole.NewLineType.Append || newLineType == Konsole.NewLineType.Both)
            {
                text += Environment.NewLine;
            }

            if (newLineType == Konsole.NewLineType.Prepend || newLineType == Konsole.NewLineType.Both)
            {
                text = Environment.NewLine + text;
            }

            list.Add(text);
        }

        public static void Add(this List<string> list, object obj, Konsole.NewLineType newLineType = Konsole.NewLineType.None)
        {
            list.Add(obj, null, newLineType);
        }

        public static string AppendLine(this object obj, object append, ConsoleColor color)
        {
            return obj.AppendLine(append.InsertTag(color));
        }

        public static string InsertTag(this object obj, ConsoleColor? color = null)
        {            
            return Color.InsertTag(obj, color, false);
        }
    }

    public partial class Konsole
    {
        public enum OperationMethod
        {
            Write,
            WriteLine,
            Read,
            ReadLine,
            WriteLog,
            PreviousOperation
        };

        public enum WriteLogParameters
        {
            WriteToConsole,
            WriteToFile,
            Truncate
        }

        public string Name { get; private set; }
        static int CursorLeft { get => Console.CursorLeft; set => Console.CursorLeft = value; }
        static int WindowWidth { get => Console.WindowWidth; }
        
        public Konsole(string instanceName)
        {
            Name = instanceName;
            Color = new Parameters.Color(this);
            Prefix = new Parameters.Prefix(this);
            NewLine = new Parameters.NewLine(this);
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

        void Print(string text, OperationMethod method, params object[] objectArray)
        {
            Log.Initialize(this, method);

            if (method.ToString().Contains("Write"))
            {
                var stringList = new List<string>();
                var colorSplitter = new Color.SplitParameters();
                var wordWrap = WordWrap.Enabled;
                bool overridePrefix = false;

                PrefixType prefix = Prefix.Current;
                NewLineType newline = (method == OperationMethod.Write) ? NewLine.Write : NewLine.WriteLine;
                ConsoleColor color = (Color.ForceColorReset) ? Color.PrimaryColor : Color.ForegroundColor;

                foreach (object obj in objectArray)
                {
                    if (stringList.Count == 0)
                    {
                        if (obj is WordWrap _wordWrap)
                        {
                            wordWrap = _wordWrap;
                        }
                        else if (obj is NewLineType _newline)
                        {
                            newline = (method == OperationMethod.Write) ? NewLine.OverrideWrite = _newline : NewLine.OverrideWriteLine = _newline;
                        }
                        else if (obj is PrefixType _prefix)
                        {
                            overridePrefix = true;
                            prefix = Prefix.Override = _prefix;
                            newline = (method == OperationMethod.Write) ? NewLine.OverrideWrite : NewLine.OverrideWriteLine;
                        }
                        else if (obj is ConsoleColor _color)
                        {
                            color = _color;
                        }
                        else if (obj is Color.SplitParameters _splitParameters)
                        {
                            colorSplitter = _splitParameters;
                        }
                        else
                        {
                            stringList.Add(obj.ToString());
                        }
                    }
                    else
                    {
                        stringList.Add(obj.ToString());
                    }
                }

                text = (stringList.Count > 0) ? text.Format(stringList.ToArray()) : text;

                text = (colorSplitter.Method != SplitMethod.None) ? colorSplitter.Execute(text) : text;

                text = (wordWrap == WordWrap.Enabled) ? Parameters.NewLine.WordWrap(text, prefix, Prefix.GetPrefix(prefix)) : text;

                if (overridePrefix)
                {
                    text = Prefix.Insert(text, prefix);
                }
                else
                {
                    if (CursorLeft == 0 || (CursorLeft > 0 && (newline == NewLineType.Prepend || newline == NewLineType.Both)))
                    {
                        text = Prefix.Insert(text);
                    }
                }

                text = NewLine.Insert(text, newline);

                Painter(text, this, color, method);
            }

            Color.Reset();
        }

        static void Painter(string text, Konsole konsole, ConsoleColor color, OperationMethod method)
        {
            string[] textArray = Color.Split(text);

            for (int i = 0; i < textArray.Length; i++)
            {
                if (color != Color.ForegroundColor && !Color.StartsWithTag(textArray[i], konsole))
                {
                    textArray[i] = textArray[i].InsertTag(color);
                }

                Console.Write(Color.Paint(textArray[i], konsole));

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

        static void Painter(string text, Konsole konsole, bool writeline = true)
        {
            Painter(Color.Split(text), konsole, writeline);
        }

        static void Painter(IEnumerable<string> stringEnumerable, Konsole konsole, bool writeline = true)
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
            return " " + new string(c, WindowWidth - 2);
        }

        public static void WriteLog(Konsole konsole, params WriteLogParameters[] parameters)
        {
            Console.ResetColor();
            Console.WriteLine();
            Console.Write(" Please wait. Processing log information. This might take a while...");
            
            if (parameters.Contains(WriteLogParameters.WriteToConsole))
            {
                Log.Initialize(konsole, OperationMethod.WriteLog);

                Konsole painter;
                List<Log.Record> records;
                List<string> processedRecords = new List<string>();
                Log.MaxLengths maxLengths;
                
                if (konsole != null)
                {
                    painter = konsole;
                    records = Log.FilterRecords(konsole);
                    maxLengths = new Log.MaxLengths(records);
                }
                else
                {
                    painter = MainKonsole;
                    records = Log.Records;
                    maxLengths = Log.MaxLength;
                }

                ConsoleColor tableColor = ConsoleColor.DarkGray;

                string thickDivider = HorizontalDivider('=');
                string thinDivider = HorizontalDivider('-');

                string headerText = " [KONSOLE LOG] : " + ((konsole != null) ? konsole.Name : "Full Record");
                string logStart = "LOG START";

                processedRecords.Add(thickDivider, ConsoleColor.DarkCyan, NewLineType.Append);
                processedRecords.Add(headerText + new string(' ', WindowWidth - headerText.Length - logStart.Length - 1), ConsoleColor.Cyan);
                processedRecords.Add(logStart, ConsoleColor.DarkCyan,NewLineType.Append);
                processedRecords.Add(thickDivider, ConsoleColor.DarkCyan, NewLineType.Append);

                string lastDate = "";
                int dateChanged = 0;

                foreach (Log.Record record in records)
                {
                    string row;

                    string logDate = record.StartTime.ToString("dddd, MMMM d, yyyy");

                    if (lastDate != logDate)
                    {
                        if (dateChanged > 0)
                        {
                            processedRecords.Add(thickDivider, ConsoleColor.DarkCyan, NewLineType.Append);
                        }

                        dateChanged++;

                        processedRecords.Add(" " + dateChanged + ": " + logDate, ConsoleColor.Cyan, NewLineType.Append);

                        lastDate = logDate;
                    }

                    string time = record.StartTime.ToString("HH:mm:ss:fff");
                    string elapsedTime = record.ElapsedTime.PadLeft(maxLengths.ElapsedTimes);
                    string operation = record.Operation.PadRight(maxLengths.Operations);
                    string text = (record.Operation == OperationMethod.WriteLog.ToString()) ? record.Text.Encapsulate(Encapsulator.Brackets) : record.Text;
                    string colorTag = Color.CreateTag((record.Operation != OperationMethod.PreviousOperation.ToString()) ? ConsoleColor.White : ConsoleColor.Gray);
                    string divider = Color.InsertTag(" | ", @"\|", tableColor, colorTag);
                    string operationID;
                    string iterationID;
                    string name;
                    int idLength = maxLengths.OperationIDs + maxLengths.IterationIDs + 1;

                    if (record.Operation != "")
                    {
                        processedRecords.Add(thinDivider, tableColor, NewLineType.Append);

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

                    if (konsole == null || konsole.Name == record.Name)
                    {
                        List<string> list = (konsole == null) ?
                            new List<string>() { name, time, elapsedTime, operation, operationID, iterationID, Color.DisableTags(text, konsole) } :
                            new List<string>() { time, elapsedTime, operation, operationID, iterationID, Color.DisableTags(text, konsole) };

                        int logCountLength = records.Count.GetLength();

                        row = " " + record.ProcessID.ToString(new string('#', logCountLength)).PadLeft(logCountLength);

                        for (int j = 0; j < list.Count; j++)
                        {
                            row += divider + list[j];                         
                        }

                        if (parameters.Contains(WriteLogParameters.Truncate) && Color.CleanTags(row, konsole).Length > Console.WindowWidth)
                        {
                            int dividerLength = ((divider.Length * list.Count) - list.Count) - ((Color.CleanTags(divider, konsole).Length * list.Count) - list.Count);
                            int trimLength = WindowWidth + dividerLength;
                            int textLength = maxLengths.Texts.GetLength();
                            string trimmed = row.Substring(trimLength - textLength - 3);
                            string endMark = ("+" + (trimmed.Length - trimmed.Count(@"\n"))).PadLeft(textLength + 1, ' ').Encapsulate(Encapsulator.Brackets);
                            row = row.Substring(0, trimLength - endMark.Length) + endMark.InsertTag(tableColor);
                        }

                        row = colorTag + Color.InsertTag(row, @"\\n|<\\\w+>", tableColor, colorTag);

                        processedRecords.AddRange(Color.Split(row + ((record != records.Last()) ? Environment.NewLine : "")));
                    }
                }

                Console.WriteLine();
                Painter(processedRecords, painter);

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

                Painter(footer, painter);
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
