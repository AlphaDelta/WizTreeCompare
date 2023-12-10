using System.Diagnostics;

namespace WizTreeCompare
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //args = new string[] { "help" };
            bool probemode = args.Length >= 1 && args[0].Contains('p');

            var prevfg = Console.ForegroundColor;
            var prevbg = Console.BackgroundColor;

            if (!probemode)
            {
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.Cyan;
                Console.Write($" WizTreeCompare ");
                Console.ForegroundColor = prevfg;
                Console.BackgroundColor = prevbg;
                Console.Write($" ");
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.WriteLine($" v{typeof(FormMain).Assembly.GetName().Version.ToString(3)}-RC.4 ");
                Console.ForegroundColor = prevfg;
                Console.BackgroundColor = prevbg;
                Console.WriteLine();
            }

            if (args.Length < 1)
            {
                nint MAIN_WINDOW = Process.GetCurrentProcess().MainWindowHandle;

                ApplicationConfiguration.Initialize();
                Application.Run(new FormMain(MAIN_WINDOW));
            }
            else if (args.Length == 3)
            {
                WTComparer comparer = new WTComparer(args[0], args[1], true);
                comparer.CompareAndSave(args[2]);
            }
            else if (args.Length == 4)
            {
                WTComparer comparer = new WTComparer(args[1], args[2], true)
                {
                    Dry = args[0].Contains('d'),
                    ProbeMode = args[0].Contains('p'),
                    IncludeNegatives = args[0].Contains('f') || args[0].Contains('n'),
                    IncludeUnchanged = args[0].Contains('f') || args[0].Contains('u'),
                    IncludeDirectories = args[0].Contains('f') || args[0].Contains('D'),
                    ForceYes = args[0].Contains('Y'),
                };
                comparer.CompareAndSave(args[3]);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.White;
                Console.WriteLine("Usage:");
                Console.ForegroundColor = prevfg;
                Console.BackgroundColor = prevbg;
                Console.WriteLine("WizTreeCompare [options] <past csv> <future csv> <output csv>");
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.White;
                Console.WriteLine("Options:");
                Console.ForegroundColor = prevfg;
                Console.BackgroundColor = prevbg;
                Console.WriteLine("d - Dry run");
                Console.WriteLine("p - Probe (reduced verbosity for easier post-processing, consider using 'Y' as well)");
                Console.WriteLine("f - Full differential (implies 'n', 'u', and 'D')");
                Console.WriteLine("n - Include negative differences (deleted files and reduced/trunacted file sizes)");
                Console.WriteLine("u - Include zero differences (unchanged file sizes)");
                Console.WriteLine("D - Include directories (without this option directory entries are skipped by default)");
                Console.WriteLine("Y - Force yes on all questions");
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.White;
                Console.WriteLine("Example:");
                Console.ForegroundColor = prevfg;
                Console.BackgroundColor = prevbg;
                Console.WriteLine("WizTreeCompare dfY \"C:\\past.csv\" \"C:\\future.csv\" \"C:\\output.csv\"");
                Console.WriteLine();

                Console.Write("Load the output CSV in WizTree via ");
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.White;
                Console.WriteLine("File > Select CSV File...");
                Console.ForegroundColor = prevfg;
                Console.BackgroundColor = prevbg;

                //#if DEBUG
                //                try
                //                {
                //                    Console.ReadKey();
                //                }catch { }
                //#endif
            }
        }
    }
}