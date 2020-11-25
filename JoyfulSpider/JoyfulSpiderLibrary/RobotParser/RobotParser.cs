/*
MIT License

Copyright (c) 2020 Kyle Givler

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

using System;
using System.Net;

namespace JoyfulSpider.Library.RobotParser
{
    public class RobotParser
    {
        /// <summary>
        /// The base Uri
        /// </summary>
        public Uri BaseUri { get; private set; } = null;
        /// <summary>
        /// The name of the robots.txt file to parse
        /// </summary>
        public string RobotFileName { get; set; } = "robots.txt";
        /// <summary>
        /// The absolute Uri of the RobotsFileName
        /// </summary>
        public Uri RobotsUri
        {
            get
            {
                if (BaseUri == null)
                {
                    throw new InvalidOperationException("BaseUri is null.");
                }

                return new Uri(BaseUri, RobotFileName);
            }
        }
        /// <summary>
        /// The full text of the robots file
        /// </summary>
        public string RobotsText
        {
            get => robotsText;
            set 
            { 
                robotsText = value;
                // TODO parse the file
            }
        }

        private string robotsText = string.Empty;

        /// <summary>
        /// Construct a RobotParser with the given baseUri and RobotFileName
        /// </summary>
        /// <param name="baseUri">The best Uri</param>
        /// <param name="robotFileName">The filename to download and parse</param>
        public RobotParser(Uri baseUri, string robotFileName) : this(baseUri)
        {
            RobotFileName = robotFileName;
        }

        /// <summary>
        /// Construct a RobotParser with the given baseUri
        /// </summary>
        /// <param name="baseUri"></param>
        public RobotParser(Uri baseUri)
        {
            BaseUri = baseUri;
            DownloadRobotsTXT();
            // TODO parse the file
        }

        public RobotParser(string robotsFileText)
        {
            // TODO parse the file
        }

        /// <summary>
        /// Download the robots.txt file
        /// </summary>
        public void DownloadRobotsTXT()
        {
            WebClient wc = new WebClient();

            try
            {
                WebClientHelper.AddHeaders(wc);

                RobotsText = wc.DownloadString(RobotsUri);
            } catch (Exception e)
            {
                ErrorHandler.ReportErrorOnConsoleAndQuit("DownloadRobotsTXT exception caught:", e); // TODO Better logging/Error handling
            }
        }

    }
}
