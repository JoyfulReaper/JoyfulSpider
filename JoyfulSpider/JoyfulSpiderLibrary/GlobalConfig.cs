/*
MIT License

Copyright (c) 2020 Kyle Givler
http://github.com/JoyfulReaper/JoyfulSpider

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using JoyfulSpider.Library.DataAccess;
using log4net;
using System.Reflection;
using log4net.Repository;
using log4net.Config;
using System.IO;

namespace JoyfulSpider.Library
{
    public static class GlobalConfig
    {
        /// <summary>
        /// Database connection
        /// </summary>
        public static IDataConnection Connection { get; private set; }
        /// <summary>
        /// Robot's User-Agent
        /// </summary>
        public static string UserAgent { get; set; } = "JoyfulSpider Alpha";
        /// <summary>
        /// Follow robots.txt rules
        /// </summary>
        public static bool FollowRobotRuels { get; set; } = true;

        /// <summary>
        /// Needed for log4net
        /// </summary>
        private static ILoggerRepository logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());


        static GlobalConfig()
        {
            // Setup log4net
            var configFile = new FileInfo("log4net.config");
            XmlConfigurator.Configure(logRepository, configFile); 
        }

        /// <summary>
        /// Connect to the database
        /// </summary>
        /// <param name="type">Database type</param>
        public static void InitiliazeDataAccess(DataAccessType type)
        {
            if(type == DataAccessType.MSSQL)
            {
                MSSQLConnector connector = new MSSQLConnector();
                Connection = connector;
            }
        }

        /// <summary>
        /// Get a log object
        /// </summary>
        /// <param name="name">Name of the logger</param>
        /// <returns>Log object</returns>
        public static ILog GetLogger(string name) => LogManager.GetLogger(name);

        /// <summary>
        /// Get a log obeject
        /// </summary>
        /// <returns>Log Object with default name</returns>
        public static ILog GetLogger() => LogManager.GetLogger("JoyfulSpider");
    }
}
