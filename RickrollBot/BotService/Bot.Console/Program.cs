
using RickrollBot.Services.ServiceSetup;
using System;
using System.Diagnostics;
using System.Reflection;

namespace RickrollBot.Console
{
    /// <summary>
    /// Class Program.
    /// Implements the <see cref="RickrollBot.Services.ServiceSetup.AppHost" />
    /// </summary>
    /// <seealso cref="RickrollBot.Services.ServiceSetup.AppHost" />
    public class Program : AppHost
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            var info = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

            if (args.Length > 0 && args[0].Equals("-v"))
            {
                System.Console.WriteLine(info.FileVersion);
                return;
            }

            var bot = new Program();

#if !DEBUG
            try
            {
#endif
                System.Console.WriteLine("RickrollBot: booting");

                bot.Boot();
                bot.StartServer();

                System.Console.WriteLine("RickrollBot: running");
#if !DEBUG
            }
            catch (Exception e)
            {
                ExceptionHandler(e);
            }
#endif

        }

        /// <summary>
        /// The exception message formatter in the console window
        /// </summary>
        /// <param name="e">The e.</param>
        public static void ExceptionHandler(Exception e)
        {
            System.Console.BackgroundColor = ConsoleColor.Black;
            System.Console.ForegroundColor = ConsoleColor.DarkRed;
            System.Console.WriteLine($"Unhandled exception: {e.Message}");
            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.WriteLine("Exception Details:");
            System.Console.ForegroundColor = ConsoleColor.DarkRed;
            InnerExceptionHandler(e.InnerException);
            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.WriteLine("press any key to exit...");
            System.Console.ReadKey();
        }

        /// <summary>
        /// Inners the exception handler.
        /// </summary>
        /// <param name="e">The e.</param>
        private static void InnerExceptionHandler(Exception e)
        {
            if (e == null) return; // return to the caller method
            System.Console.WriteLine(e.Message);
            // Call recursively to output all inner exception messages
            InnerExceptionHandler(e.InnerException);
        }
    }
}
