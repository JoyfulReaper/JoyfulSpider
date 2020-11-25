// Just for testing

using System;

namespace JoyfulSpiderLibrary
{
    public static class ErrorHandler
    {
        public static void ReportErrorOnConsoleAndQuit(string error, Exception e)
        {
            ConsoleColor old = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine($"ERROR: {error}");

            if (e != null)
            {
                Console.WriteLine(e.Message);
            }

            Console.ForegroundColor = old;
            Console.WriteLine();

            Environment.Exit(-1);
        }
    }
}
