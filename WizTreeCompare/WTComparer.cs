using CsvHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizTreeCompare
{
    public class WTComparer
    {
        enum LogType { Info, Warning, Error }

        string PastPath, FuturePath;
        bool IsConsole;
        public WTComparer(string pastpath, string futurepath, bool console = false)
        {
            this.PastPath = pastpath;
            this.FuturePath = futurepath;
            this.IsConsole = console;
        }

        ConsoleColor prevfg;
        ConsoleColor prevbg;

        public void CompareAndSave(string outputpath)
        {
            prevfg = Console.ForegroundColor;
            prevbg = Console.BackgroundColor;

            if (!File.Exists(PastPath))
            {
                Log("Past CSV could not be found, make sure it hasn't been moved or deleted", LogType.Error);
                return;
            }

            if (!File.Exists(FuturePath))
            {
                Log("Future CSV could not be found, make sure it hasn't been moved or deleted", LogType.Error);
                return;
            }

            if (File.Exists(outputpath))
            {
                if (!IsConsole)
                {
                    if (MessageBox.Show($"A file already exists at '{outputpath}'. Would you like you overwrite it?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                        return;
                }
                else
                {
                    Log($"The output file '{outputpath}' already exists", LogType.Warning);
                    Log($"Would you like to overwrite it? (y/N)", LogType.Warning, false);
                    string line = Console.ReadLine();
                    if (String.IsNullOrWhiteSpace(line) || (line[0] != 'y' && line[0] != 'Y'))
                        return;
                }
            }

            LogToConsole("Reading past CSV and populating dictionary...");
            Dictionary<string, WTCsvRow> pastrows = new Dictionary<string, WTCsvRow>();
            using (var sr = new StreamReader(PastPath, Encoding.UTF8))
            {
                string line = sr.ReadLine();
                if (line != null && line[0] != 'G')
                    sr.BaseStream.Seek(0, SeekOrigin.Begin);

                using (var csv = new CsvReader(sr, System.Globalization.CultureInfo.InvariantCulture))
                {
                    foreach (WTCsvRow row in csv.GetRecords<WTCsvRow>())
                    {
                        if (row.FileName.EndsWith('\\') || row.FileName.EndsWith('/'))
                            continue; // It's a folder, we don't care about folders

                        pastrows[row.FileName] = row;
                    }
                }
            }

            LogToConsole("Reading future CSV and populating output differential file...");
            using (var sr = new StreamReader(FuturePath, Encoding.UTF8))
            {
                string line = sr.ReadLine();
                if (line != null && line[0] != 'G')
                    sr.BaseStream.Seek(0, SeekOrigin.Begin);

                using (var csv = new CsvReader(sr, System.Globalization.CultureInfo.InvariantCulture))
                using (var writer = new StreamWriter(outputpath, false, Encoding.UTF8))
                using (var csvoutput = new CsvWriter(writer, new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
                {
                    ShouldQuote = (e) => e.FieldType == typeof(string)
                }))
                {
                    csvoutput.WriteHeader<WTCsvRow>();
                    foreach (WTCsvRow futurerow in csv.GetRecords<WTCsvRow>())
                    {
                        if (futurerow.FileName.EndsWith('\\') || futurerow.FileName.EndsWith('/'))
                            continue; // It's a folder, we don't care about folders

                        if (!pastrows.ContainsKey(futurerow.FileName))
                        {
                            csvoutput.NextRecord();
                            csvoutput.WriteRecord(futurerow);
                        }
                        else
                        {
                            WTCsvRow pastrow = pastrows[futurerow.FileName];

                            long size = futurerow.Size - pastrow.Size;
                            if (size < 1) continue;

                            long allocated = futurerow.Allocated - pastrow.Allocated;
                            if (allocated < 0) allocated = 0; //Allocated chunks can be removed despite file being logically larger -- What a weird bug

                            WTCsvRow newrow = new WTCsvRow();
                            newrow.FileName = futurerow.FileName;
                            newrow.Size = size;
                            newrow.Allocated = allocated;
                            newrow.Modified = futurerow.Modified;
                            newrow.Attributes = futurerow.Attributes;

                            csvoutput.NextRecord();
                            csvoutput.WriteRecord(newrow);
                        }
                    }
                }
            }

            Log($"Differential complete at '{outputpath}'");
        }

        void LogToConsole(string msg, LogType type = LogType.Info, bool writeline = true)
        {
            switch (type)
            {
                default:
                case LogType.Info:
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.Cyan;
                    Console.Write($" i ");
                    break;
                case LogType.Warning:
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.Write($" ! ");
                    break;
                case LogType.Error:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    Console.Write($"!!!");
                    break;
            }
            Console.ForegroundColor = prevfg;
            Console.BackgroundColor = prevbg;
            Console.Write($" ");
            Console.Write(msg);
            if (writeline)
                Console.WriteLine();
        }

        void Log(string msg, LogType type = LogType.Info, bool writeline = true)
        {
            if (IsConsole)
            {
                LogToConsole(msg, type, writeline);
            }
            else
            {
                switch (type)
                {
                    default:
                    case LogType.Info:
                        MessageBox.Show(msg, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    case LogType.Warning:
                        MessageBox.Show(msg, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;
                    case LogType.Error:
                        MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                }
            }
        }
    }
}
