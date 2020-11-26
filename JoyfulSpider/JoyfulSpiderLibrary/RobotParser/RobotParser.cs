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

/*
References: 
https://moz.com/learn/seo/robotstxt
http://www.robotstxt.org/orig.html
https://en.wikipedia.org/wiki/Robots_exclusion_standard
*/

// https://en.wikipedia.org/wiki/Robots_exclusion_standard#Nonstandard_extensions
// TODO: Nonstandard extensions: Crawl-Delay, Sitemap

// Also note Pattern-Matching is not reconginzed by the standard as far as I know
// https://moz.com/learn/seo/robotstxt (See Pattern-matching section)
// * is a wildcard
// $ matches the end of the url
// Ex: 
//   Allow: /alerts/$
//   Disallow: / intl/*/about/views/

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
                if(!RobotsValid)
                {
                    return true;
                }

                if(HonorWildCardDisallow && foundWildcardDisallow)
                {
                    return false;
                }

                return allowedUris.Count > 0 && RootDisallowed == false;
            }
        }

        /// <summary>
        /// If true honor the non-standard Disallow: * rule, if false ignore it.
        /// </summary>
        public bool HonorWildCardDisallow { get; set; } = false;

        /// <summary>
        /// The date and time the robots file was downloaded
        /// </summary>
        public DateTimeOffset? RobotFetchDate { get; private set; } = null;

        private readonly List<Uri> disallowedUris = new List<Uri>();
        private readonly List<Uri> allowedUris = new List<Uri>();

        private bool foundWildcardDisallow = false;
        private bool RobotsValid = false;

        private readonly ILog logger = GlobalConfig.GetLogger("RobotParser");

        /// <summary>
        /// Construct a RobotParser with the given baseUri and RobotFileName
        /// </summary>
        /// <param name="baseUri">The base Uri</param>
        /// <param name="robotFileName">The filename to download and parse</param>
        public RobotParser(Uri baseUri, string robotFileName) : this(baseUri)
        {
            logger.Debug($"ctor: RobotParser(Uri {baseUri}, string {robotFileName}) : this(baseUri)");

            RobotFileName = robotFileName;
        }

        /// <summary>
        /// Construct a RobotParser with the given baseUri
        /// </summary>
        /// <param name="baseUri">base Uri</param>
        public RobotParser(Uri baseUri)
        {
            logger.Debug($"ctor: RobotParser(Uri {baseUri})");

            BaseUri = baseUri;
            DownloadRobotsTXT();
            Parse();
        }

        /// <summary>
        /// Construct a RobotParser with the given baseUri and robots file contents
        /// </summary>
        /// <param name="robotsFileText">Full text of the robots.txt file</param>
        /// <param name="baseUri">base Uri</param>
        public RobotParser(string robotsFileText, Uri baseUri)
        {
            logger.Debug($"ctor: RobotParser(string robotsFileText, Uri {baseUri})");

            BaseUri = baseUri;
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
                RobotFetchDate = DateTimeOffset.Now;
            } 
            catch (Exception e)
            {
                ErrorHandler.ReportErrorAndQuit("DownloadRobotsTXT() exception caught:", e); // TODO Better logging/Error handling
            }

            logger.Info("DownloadRobotsTXT(): robots.txt file downloaded successfully.");
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

            bool foundrule = false;

            using (StringReader reader = new StringReader(robotsText))
            {
                string line = String.Empty;
                string currentAgent = string.Empty;

                line = reader.ReadLine();

                while (line != null)
                {
                    if (line.StartsWith("User-agent")) // Found User-agent entry
                    {
                        foundrule = true;
                        currentAgent = line.Split(' ')[1];

                        logger.Info($"Parse(robotsText): Found User-agent: {currentAgent}");
                    }
                    else if (line.StartsWith("Allow") || line.StartsWith("Disallow")) // Found Allow or Disallow entry
                    {
                        logger.Debug($"Parse(robotsText): Found Allow/Disallow Rule for {currentAgent}");

                        if (currentAgent == "*" || currentAgent == GlobalConfig.UserAgent) // The rule applies to us
                        {
                            if (line != null)
                            {
                                ProccessAllowOrDisallow(line);
                            }
                        }
                    }

                    line = reader.ReadLine();
                }

                if(!foundrule) // No rules were found
                {
                    RobotsValid = false;
                }
            }
        }

        /// <summary>
        /// Checks the robots.txt files for any relevant allows or disallows
        /// </summary>
        /// <param name="line">The rule to process</param>
        private void ProccessAllowOrDisallow(string line)
        {
            logger.Debug("ProccessAllowOrDisallow(string line)");

            if (line.StartsWith("Allow:"))
            {
                ProcessAllowRule(line);
            }
            else if (line.StartsWith("Disallow:"))
            {
                ProcessDisallowRule(line);
            }
            else
            {
                logger.Warn($"ProccessAllowOrDisallow(): {line} is not an allow/disallow rule");
            }
        }

        private void ProcessAllowRule(string line)
        {
            logger.Debug($"ProcessAllow(string {line})");

            string lineAllowed = line.Split(' ')[1];

            if (lineAllowed == "/")
            {
                logger.Info("ProcessAllow(line): Ignoring rule for /");
                return;
            }

            Uri UriAllowed = new Uri(BaseUri, lineAllowed);

            if (!allowedUris.Contains(UriAllowed))
            {
                if (IsRuleSupported(UriAllowed))
                {
                    allowedUris.Add(UriAllowed);
                    logger.Debug($"ProcessAllow(line): Added Allow rule: Allow: {lineAllowed}");
                }
            }
            else
            {
                logger.Debug($"ProcessAllow(line): Rule already existed: Allow: {lineAllowed}");
            }
        }

        private void ProcessDisallowRule(string line)
        {
            logger.Debug($"ProcessDisallow(string {line})");

            string lineDisallowed = line.Split(' ')[1];

            if (lineDisallowed == "*")
            {
                // Note "*" is non-standard. (https://en.wikipedia.org/wiki/Robots_exclusion_standard#Nonstandard_extensions)
                // We ignore these rules unless HonorWildcardDisallow == true!
                foundWildcardDisallow = true;

                logger.Warn("ProcessDisallow(line): Found Disallow *, which is not standards compliant!");
            }

            if (lineDisallowed == "/")
            {
                RootDisallowed = true;
                logger.Info("ProcessDisallow(line): Found root (/) Disallow rule");
            }

            Uri UriDisallowed = new Uri(BaseUri, lineDisallowed);

            if (!disallowedUris.Contains(UriDisallowed))
            {
                if (IsRuleSupported(UriDisallowed))
                {
                    disallowedUris.Add(UriDisallowed);
                    logger.Debug($"ProcessDisallow(line): Added Disallow rule: Disallow: {lineDisallowed}");
                }
            }
            else
            {
                logger.Debug($"ProcessDisallow(line): Rule already existed: Disallow: {lineDisallowed}");
            }
        }

        public bool Allowed(Uri targetUri) // I am not 100% convinced the logic is correct, however this seems to work well enough for now
        {
            logger.Debug($"Allowed(Uri {targetUri})");
            bool allowed = true;

            if(BaseUri.Host != targetUri.Host)
            {
                logger.Error($"Allowed(targetUri): {BaseUri.Host} != {targetUri.Host}");
            }

            // Check if we are not allowed to crawl anything
            if (!AnyAllowed)
            {
                logger.Info("Allowed(targetUri): We are not allowed to crawl any Uris!");
                return false;
            }

            // Check if we are explicitly disallowed to crawl
            foreach (var u in disallowedUris)
            {
                string absoulutePath = u.AbsolutePath;
                string targetAbsolutePath = targetUri.AbsolutePath;

                if(targetAbsolutePath.Contains(absoulutePath))
                {
                    allowed = false;
                    break;
                }
            }

            // Check if we are explicitly allowed to crawl
            foreach (var u in allowedUris)
            {
                if (u == targetUri)
                {
                    allowed = true;
                    break;
                }

                string absoulutePath = u.AbsolutePath;
                string targetAbsoulutePath = targetUri.AbsolutePath;

                if (targetAbsoulutePath.EndsWith(u.AbsolutePath))
                {
                    allowed = true;
                    break;
                }
            }

            logger.Debug($"Allowed(targetUri): We {(allowed ? "are" : "are NOT")} allowed to crawl {targetUri}");
            return allowed;
        }

        // Check if we don't (currently) handle/support these rules!
        // TODO: Support the damn rules already!
        private bool IsRuleSupported(Uri uri)
        {
            logger.Debug($"IsRuleSupported({uri})");
            bool supported = true;

            if(!string.IsNullOrEmpty(uri.Fragment))
            {
                supported = false;
                logger.Warn($"IsRuleSupported(uri): {uri} not supported: contains Fragment");
            }

            if(!string.IsNullOrEmpty(uri.Query))
            {
                supported = false;
                logger.Warn($"IsRuleSupported(uri): {uri} not supported: contains Query");
            }

            return supported;
        }

    }
}
