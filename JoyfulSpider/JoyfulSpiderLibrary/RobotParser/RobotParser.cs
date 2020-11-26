﻿/*
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

// References: https://moz.com/learn/seo/robotstxt
// http://www.robotstxt.org/orig.html

using log4net;
using System;
using System.Collections.Generic;
using System.IO;
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
                //Parse(); Don't parse automaticly (was double parsing)
            }
        }
        /// <summary>
        /// The full text of the robots.txt file
        /// </summary>
        private string robotsText = string.Empty;
        /// <summary>
        /// True if there is a Disallow: / rule
        /// </summary>
        public bool RootDisallowed { get; private set; }
        /// <summary>
        /// True if the spider has access to ANY links
        /// </summary>
        public bool AnyAllowed
        {
            get
            {
                return allowed.Count > 0 && RootDisallowed == false;
            }
        }

        private List<Uri> disallowed = new List<Uri>();
        private List<Uri> allowed = new List<Uri>();
        private ILog logger = GlobalConfig.GetLogger("RobotParser");


        /// <summary>
        /// Construct a RobotParser with the given baseUri and RobotFileName
        /// </summary>
        /// <param name="baseUri">The best Uri</param>
        /// <param name="robotFileName">The filename to download and parse</param>
        public RobotParser(Uri baseUri, string robotFileName) : this(baseUri)
        {
            logger.Debug($"ctor: RobotParser(Uri {baseUri}, string {robotFileName}) : this(baseUri)");

            RobotFileName = robotFileName;
        }

        /// <summary>
        /// Construct a RobotParser with the given baseUri
        /// </summary>
        /// <param name="baseUri"></param>
        public RobotParser(Uri baseUri)
        {
            logger.Debug($"ctor: RobotParser(Uri {baseUri})");

            BaseUri = baseUri;
            DownloadRobotsTXT();
            Parse();
        }

        /// <summary>
        /// Construct a RobotParser with the given robots.txt text
        /// </summary>
        /// <param name="robotsFileText">Robots.txt text</param>
        public RobotParser(string robotsFileText)
        {
            logger.Debug($"ctor: RobotParser(string {robotsFileText})");

            RobotsText = robotsFileText;
            Parse();
        }

        /// <summary>
        /// Download the robots.txt file
        /// </summary>
        public void DownloadRobotsTXT()
        {
            logger.Debug("DownloadRobotsTXT()");

            WebClient wc = new WebClient();

            try
            {
                WebClientHelper.AddHeaders(wc);

                RobotsText = wc.DownloadString(RobotsUri);
            } 
            catch (Exception e)
            {
                ErrorHandler.ReportErrorOnConsoleAndQuit("DownloadRobotsTXT() exception caught:", e); // TODO Better logging/Error handling
            }

            logger.Debug("DownloadRobotsTXT(): robots.txt file downloaded successfully.");
        }

        public void Parse()
        {
            logger.Debug("Parse()");

            Parse(robotsText);
        }

        // TODO: Currently only Parses Allow and Disallow
        /// <summary>
        /// Parses the robots.txt rules
        /// </summary>
        /// <param name="robotsText"></param>
        public void Parse(string robotsText)
        {
            logger.Debug("Prase(robotsText)");

            using (StringReader reader = new StringReader(robotsText))
            {
                string line = String.Empty;

                while(line != null)
                {
                    line = reader.ReadLine();
                    if(line.StartsWith("User-agent:"))
                    {
                        string agent = line.Split(' ')[1];
                        logger.Debug($"Found User-agent: {agent}");

                        if (agent == "*" || agent == GlobalConfig.UserAgent)
                        {
                            logger.Debug($"Proccessing User-agent {agent} becasue it applies to us.");

                            CheckPermissions(reader, out bool moreLines);
                            line = moreLines ? string.Empty : null;
                        }
                        else
                        {
                            logger.Debug($"Ignoring rules for {agent}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks the robots.txt files for any relevant allows or disallows
        /// </summary>
        /// <param name="reader">The reader wrapping the robots txt</param>
        /// <param name="moreLines">true if there are more lines in the reader, otherwise false</param>
        private void CheckPermissions(StringReader reader, out bool moreLines)
        {
            logger.Debug("CheckPermissions(string reader read, out bool moreLines)");

            string line;

            while(true)
            {
                line = reader.ReadLine();

                if(line.StartsWith("Allow:"))
                {
                    string lineAllowed = line.Split(' ')[1];

                    Uri UriAllowed = new Uri(BaseUri, lineAllowed);

                    allowed.Add(UriAllowed);

                    logger.Debug($"Added Allow rule: Allow: {lineAllowed}");
                }
                else if(line.StartsWith("Disallow:"))
                {
                    string lineDisallowed = line.Split(' ')[1];

                    if(lineDisallowed == "/")
                    {
                        RootDisallowed = true;
                    }
                    Uri UriDisallowed = new Uri(BaseUri, lineDisallowed);
                    disallowed.Add(UriDisallowed);

                    logger.Debug($"Added Disallow rule: Disallow: {lineDisallowed}");
                }
                else
                {
                    moreLines = line == null;
                    return;
                }
            }
        }

        public bool Allowed(Uri uri)
        {
            return false;
            if(!AnyAllowed)
            {
                return false;
            }
        }
    }
}
