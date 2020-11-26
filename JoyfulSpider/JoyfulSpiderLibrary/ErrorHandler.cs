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

// We can likely re-factor some stuff to get rid of this class,
// Which was used for testing before switching to log4net
// Or keep it as an easy way to quit on a Fatal error

using System;

namespace JoyfulSpider.Library
{
    public static class ErrorHandler
    {
        public static void ReportErrorAndQuit(string message, Exception e)
        {
            var log = GlobalConfig.GetLogger("FatalErrorReporter");

            if (e != null)
            {
                log.Fatal(message, e);
            }
            else
            {
                log.Fatal(message);
            }

            Environment.Exit(-1);
        }
    }
}
