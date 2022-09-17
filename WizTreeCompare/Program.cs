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

            var prevfg = Console.ForegroundColor;
            var prevbg = Console.BackgroundColor;

            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.Cyan;
            Console.Write($" WizTreeCompare ");
            Console.ForegroundColor = prevfg;
            Console.BackgroundColor = prevbg;
            Console.Write($" ");
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.Gray;
            Console.WriteLine($" v{typeof(FormMain).Assembly.GetName().Version.ToString(3)} ");
            Console.ForegroundColor = prevfg;
            Console.BackgroundColor = prevbg;
            Console.WriteLine();

            if (args.Length < 1)
            {
                ApplicationConfiguration.Initialize();
                Application.Run(new FormMain());
            }
            else if (args.Length == 3)
            {
                WTComparer comparer = new WTComparer(args[0], args[1], true);
                comparer.CompareAndSave(args[2]);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.White;
                Console.WriteLine("Usage:");
                Console.ForegroundColor = prevfg;
                Console.BackgroundColor = prevbg;
                Console.WriteLine("WizTreeCompare <past csv> <future csv> <output csv>");
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.White;
                Console.WriteLine("Example:");
                Console.ForegroundColor = prevfg;
                Console.BackgroundColor = prevbg;
                Console.WriteLine("WizTreeCompare \"C:\\past.csv\" \"C:\\future.csv\" \"C:\\output.csv\"");
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