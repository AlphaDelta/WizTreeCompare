using CsvHelper;
using CsvHelper.TypeConversion;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WizTreeCompare
{
    public class WTComparer
    {
        string PastPath, FuturePath;

        public bool IsConsole,
            IncludeNegatives = false,
            Dry = false,
            ForceYes = false,
            IncludeUnchanged = false,
            ProbeMode = false,
            IncludeDirectories = false
        ;

        public StreamWriter Stream = StreamWriter.Null;

        public CancellationToken CancellationToken;
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

            if (File.Exists(outputpath) && !Dry)
            {
                if (!IsConsole)
                {
                    //if (MessageBox.Show($"A file already exists at '{outputpath}'. Would you like you overwrite it?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    //    return;
                }
                else if (ForceYes)
                {
                    Log($"Force yes is on... the output file '{outputpath}' exists but will be overwritten", LogType.Warning);
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

            /* Set up progress logger */
            int nrow = 0;
            TimeSpan tickrate = TimeSpan.FromMilliseconds(300);
            var action = (ProgressContextConsole ctx) => { };
            if (!ProbeMode)
                action = (ProgressContextConsole ctx) =>
                {
                    if (ctx.ProgressCurrent >= 0 && ctx.ProgressTotal > 0)
                        ctx.AnnounceProgress($"Accessing row {nrow}, {(ctx.ProgressCurrent / ctx.ProgressTotal) * 100:0.00}% complete, running for {(DateTime.Now - ctx.StartTime):m'm 's's 'fff'ms'}");
                    else
                        ctx.AnnounceProgress($"Accessing row {nrow}, running for {(DateTime.Now - ctx.StartTime):m'm 's's 'fff'ms'}");
                };


            /* PAST CSV */
            LogToConsole("Reading past CSV and populating dictionary...");
            Dictionary<string, WTCsvRow> pastrows = new Dictionary<string, WTCsvRow>();
            if (CancellationToken.IsCancellationRequested) return; // <X>
            using (var progress = new ProgressContextConsole(tickrate, action))
            using (var sr = new StreamReader(PastPath, Encoding.UTF8))
            using (var csv = new CsvReader(sr, System.Globalization.CultureInfo.InvariantCulture))
            {
                csv.Read();
                if (csv.Context.Parser.RawRecord.ToLower().TrimStart("\"'".ToCharArray()).StartsWith("generated"))
                    csv.Read();
                csv.ReadHeader();

                progress.StartTime = DateTime.Now;
                progress.ProgressTotal = sr.BaseStream.CanSeek ? sr.BaseStream.Length : -1;
                progress.Action(progress);

                foreach (WTCsvRow row in csv.GetRecords<WTCsvRow>())
                {
                    if (CancellationToken.IsCancellationRequested) return; // <X>

                    /* Progress */
                    nrow = csv.Context.Parser.Row;
                    progress.ProgressCurrent = progress.ProgressTotal > 0 ? sr.BaseStream.Position : -1;
                    progress.InvokeLater();

                    /* Work */
                    if (!IncludeDirectories && row.IsDirectory)
                        continue; // It's a folder and we don't care

                    pastrows[row.FileName] = row;
                }
            }
            LogToConsole($"Past dictionary populated with {pastrows.Count} entries", LogType.InfoImportant);

            /* FUTURE CSV subtract PAST CSV */
            LogToConsole("Reading future CSV and populating output differential file...");
            int additions = 0, modifications = 0, deletions = 0, nochange = 0;
            long addbyte = 0, subbyte = 0;
            int diradditions = 0, dirmodifications = 0, dirdeletions = 0, dirnochange = 0;
            long diraddbyte = 0, dirsubbyte = 0;

            //using () //Todo: fix this
            using (var writer = Stream != StreamWriter.Null ? Stream : new StreamWriter(outputpath, false, Encoding.UTF8))
            using (var csvoutput = new CsvWriter(writer, new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
            {
                ShouldQuote = (e) => e.FieldType == typeof(string)
            }))
            {
                HashSet<string> seen = new HashSet<string>(pastrows.Count);

                using (var progress = new ProgressContextConsole(tickrate, action))
                using (var sr = new StreamReader(FuturePath, Encoding.UTF8))
                {
                    /* Progress */
                    progress.StartTime = DateTime.Now;
                    progress.ProgressTotal = sr.BaseStream.CanSeek ? sr.BaseStream.Length : -1;
                    progress.Action(progress);

                    /* Work */
                    using (var csv = new CsvReader(sr, System.Globalization.CultureInfo.InvariantCulture))
                    {
                        csv.Read();
                        if (csv.Context.Parser.RawRecord.ToLower().TrimStart("\"'".ToCharArray()).StartsWith("generated"))
                            csv.Read();
                        csv.ReadHeader();

                        var options = new TypeConverterOptions { Formats = new[] { "yyyy-MM-dd HH:mm:ss" } };
                        csvoutput.Context.TypeConverterOptionsCache.AddOptions<DateTime>(options);
                        csvoutput.Context.TypeConverterOptionsCache.AddOptions<DateTime?>(options);

                        csvoutput.WriteHeader<WTCsvRow>();
                        foreach (WTCsvRow futurerow in csv.GetRecords<WTCsvRow>())
                        {
                            if (CancellationToken.IsCancellationRequested) return; // <X>

                            /* Progress */
                            nrow = csv.Context.Parser.Row;
                            progress.ProgressCurrent = progress.ProgressTotal > 0 ? sr.BaseStream.Position : -1;
                            progress.InvokeLater();

                            /* Work */
                            if (!IncludeDirectories && futurerow.IsDirectory)
                                continue; // It's a folder and we don't care

                            if (!pastrows.ContainsKey(futurerow.FileName))
                            {
                                //It's a new file, add it as-is
                                csvoutput.NextRecord();
                                csvoutput.WriteRecord(futurerow);
                                if (!futurerow.IsDirectory)
                                {
                                    //File
                                    additions++;
                                    addbyte += futurerow.Size;
                                }
                                else
                                {
                                    //Directory
                                    diradditions++;
                                    diraddbyte += futurerow.Size;
                                }
                            }
                            else
                            {
                                seen.Add(futurerow.FileName);

                                //Same file, calc difference
                                WTCsvRow pastrow = pastrows[futurerow.FileName];

                                long size = futurerow.Size - pastrow.Size;
                                if (!IncludeNegatives && size < 0) continue; //Negative size change
                                if (!IncludeUnchanged && size == 0) continue;

                                long allocated = futurerow.Allocated - pastrow.Allocated;
                                if (!IncludeNegatives && allocated < 0) allocated = 0; //Allocated chunks can be removed despite file being logically larger -- What a weird bug

                                WTCsvRow newrow = new WTCsvRow();
                                newrow.FileName = futurerow.FileName;
                                newrow.Size = size;
                                newrow.Allocated = allocated;
                                newrow.Modified = futurerow.Modified;
                                newrow.Attributes = futurerow.Attributes;

                                csvoutput.NextRecord();
                                csvoutput.WriteRecord(newrow);

                                if (!futurerow.IsDirectory)
                                {
                                    //File
                                    if (newrow.Size == 0)
                                        nochange++;
                                    else
                                        modifications++;

                                    if (newrow.Size > 0)
                                        addbyte += newrow.Size;
                                    else
                                        subbyte += -newrow.Size;
                                }
                                else
                                {
                                    //Directory
                                    if (newrow.Size == 0)
                                        dirnochange++;
                                    else
                                        dirmodifications++;

                                    if (newrow.Size > 0)
                                        diraddbyte += newrow.Size;
                                    else
                                        dirsubbyte += -newrow.Size;
                                }
                            }
                        }/*end foreach*/
                    }/*end using CsvReader*/
                }/*end using StreamReader*/
                //LogToConsole($"Populated output csv with {additions} additions and {modifications} differences", LogType.InfoImportant);

                /* Full */
                if (IncludeNegatives)
                {
                    LogToConsole("Finding and populating deletions...");

                    using (var progress = new ProgressContextConsole(tickrate, action))
                    {
                        /* Progress */
                        nrow = 0;
                        progress.StartTime = DateTime.Now;
                        progress.ProgressTotal = pastrows.Count;
                        progress.Action(progress);

                        /* Work */
                        foreach (var kv in pastrows)
                        {
                            if (CancellationToken.IsCancellationRequested) return; // <X>

                            progress.ProgressCurrent = ++nrow;
                            progress.InvokeLater();

                            if (seen.Contains(kv.Key)) continue;

                            WTCsvRow newrow = new WTCsvRow();
                            newrow.FileName = kv.Value.FileName;
                            newrow.Size = -kv.Value.Size;
                            newrow.Allocated = 0;
                            newrow.Modified = kv.Value.Modified;
                            newrow.Attributes = kv.Value.Attributes;

                            csvoutput.NextRecord();
                            csvoutput.WriteRecord(newrow);
                            if (!kv.Value.IsDirectory)
                            {
                                //File
                                deletions++;
                                subbyte += kv.Value.Size;
                            }
                            else
                            {
                                //Directory
                                dirdeletions++;
                                dirsubbyte += kv.Value.Size;
                            }
                        }
                    }

                    //LogToConsole($"Populated output csv with {deletions} deletions", LogType.InfoImportant);
                }
            }/*end using CsvWriter*/

            long bytediff = addbyte - subbyte;
            long dirbytediff = diraddbyte - dirsubbyte;
            if (!IncludeNegatives) subbyte = dirsubbyte = deletions = dirdeletions = -1;
            if (ProbeMode)
            {
                LogToConsole($"Files - {additions} additions - {deletions:0;'N/A';0} deletions - {modifications} modifications", LogType.InfoImportant);
                LogToConsole($"Bytes - {addbyte} added - {subbyte:0;'N/A';0} subtracted - {bytediff:+0;-0;0} differential", LogType.InfoImportant);
                LogToConsole($"Folders - {diradditions} additions - {dirdeletions:0;'N/A';0} deletions - {dirmodifications} modifications", LogType.InfoImportant);
                LogToConsole($"Folderbytes - {diraddbyte} added - {dirsubbyte:0;'N/A';0} subtracted - {dirbytediff:+0;-0;0} differential", LogType.InfoImportant);
            }
            else
            {

                LogToConsole($"Files - {additions:#,0} additions - {deletions:#,0;'N/A';0} deletions - {modifications:#,0} modifications", LogType.InfoImportant);
                if (!IncludeDirectories)
                    LogToConsole($"Bytes - {BytesToString(addbyte)} added - {(subbyte < 0 ? "N/A" : BytesToString(subbyte))} subtracted - {bytediff:+;'';''}{BytesToString(bytediff)} differential", LogType.InfoImportant);
                else
                {
                    LogToConsole($"Bytes (Considering files only) - {BytesToString(addbyte)} added - {(subbyte < 0 ? "N/A" : BytesToString(subbyte))} subtracted - {bytediff:+;'';''}{BytesToString(bytediff)} differential", LogType.InfoImportant);
                    LogToConsole($"Folders - {diradditions:#,0} additions - {dirdeletions:#,0;'N/A';0} deletions - {dirmodifications:#,0} modifications", LogType.InfoImportant);
                    LogToConsole($"Bytes (Considering folders only) - {BytesToString(diraddbyte)} added - {(dirsubbyte < 0 ? "N/A" : BytesToString(dirsubbyte))} subtracted - {dirbytediff:+;'';''}{BytesToString(dirbytediff)} differential", LogType.InfoImportant);
                }
            }

            if (!Dry)
                Log($"Differential complete at '{outputpath}'", LogType.Success);
            else
                Log($"Dry run complete", LogType.Success);
            Console.WriteLine();
        }

        /* ==========================
         *          Helpers
         * ==========================
         */
        public static string BytesToString(long byteCount)
        {
            //https://stackoverflow.com/a/4975942
            string[] suf = { " B", " KiB", " MiB", " GiB", " TiB", " PiB", " EiB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString("0.###") + suf[place];
        }

        enum LogType { Success = 0, Info = 1, InfoImportant = 1 | 0x08, Warning = 2 | 0x08, Error = 3 | 0x08 }
        void LogToConsole(string msg, LogType type = LogType.Info, bool writeline = true)
        {
            if (ProbeMode)
            {
                if (((int)type & 0x08) == 0x08) Console.WriteLine(msg);
                return;
            }

            switch (type)
            {
                default:
                case LogType.InfoImportant:
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.Cyan;
                    Console.Write($" i ");
                    break;
                case LogType.Info:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write($" i ");
                    break;
                case LogType.Success:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write($" ~ ");
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
            LogToConsole(msg, type, writeline);

            if (!IsConsole)
            {
                switch (type)
                {
                    default:
                    case LogType.Info:
                    case LogType.Success:
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
        }/*end Log()*/
    }
}
