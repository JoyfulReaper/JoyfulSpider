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


using JoyfulSpider.Library.RobotParser;
using System;

namespace JoyfulSpider.ConsoleUI
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleHelper.DefaultColor = ConsoleColor.DarkCyan;

            ConsoleHelper.ColorWriteLine(ConsoleColor.Cyan, "Welcome to JoyfulSpider!\n");
            ConsoleHelper.ColorWrite("Enter a Uri: ");
            string input = Console.ReadLine();

            Uri uri = new Uri(input);
            RobotParser rp = new RobotParser(uri);

            Uri baseUri = rp.BaseUri;
            while (true)
            {
                ConsoleHelper.ColorWriteLine($"Check to see if we are allowed to crawl a Uri relative to {baseUri}");
                ConsoleHelper.ColorWrite("Uri to check: ");
                var checkInput = Console.ReadLine();

                Uri checkUri = new Uri(baseUri, checkInput);

                if (rp.Allowed(checkUri))
                {
                    ConsoleHelper.ColorWriteLine(ConsoleColor.Green, $"We are allowed to crawl: {checkUri}");
                }
                else
                {
                    ConsoleHelper.ColorWriteLine(ConsoleColor.Red, $"We are NOT allowed to crawl: {checkUri}");
                } 
            }
        }
    }
}
