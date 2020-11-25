// Just for testing

using System;
using System.Reflection;
using JoyfulSpider.Library;
using log4net;

namespace JoyfulSpider.Library
{
    public static class ErrorHandler
    {
        public static void ReportErrorOnConsoleAndQuit(string message, Exception e)
        {
            var log = GlobalConfig.GetLogger();

            if (e != null)
            {
                log.Fatal(message, e);
            }
            else
            {
                log.Fatal(message);
            }

            Console.WriteLine();
            Environment.Exit(-1);
        }
    }
}
