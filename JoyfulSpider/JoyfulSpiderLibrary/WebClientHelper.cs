using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JoyfulSpider.Library
{
    public static class WebClientHelper
    {
        public static void AddHeaders(WebClient wc)
        {
            wc.Headers.Add("user-agent", GlobalConfig.UserAgent);
        }
    }
}
